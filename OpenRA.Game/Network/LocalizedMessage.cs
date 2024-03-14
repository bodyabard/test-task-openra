#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using Linguini.Shared.Types.Bundle;

namespace OpenRA.Network
{
	public class FluentArgument
	{
		public enum FluentArgumentType
		{
			String = 0,
			Number = 1,
		}

		public readonly string Key;
		public readonly string Value;
		public readonly FluentArgumentType Type;

		public FluentArgument() { }

		public FluentArgument(string key, object value)
		{
			Key = key;
			Value = value.ToString();
			Type = GetFluentArgumentType(value);
		}

		static FluentArgumentType GetFluentArgumentType(object value)
		{
			switch (value.ToFluentType())
			{
				case FluentNumber:
					return FluentArgumentType.Number;
				default:
					return FluentArgumentType.String;
			}
		}
	}

	public class LocalizedMessage
	{
		public const int ProtocolVersion = 1;

		public readonly string Key = string.Empty;

		[FieldLoader.LoadUsing(nameof(LoadArguments))]
		public readonly Dictionary<string, object> Arguments;

		static object LoadArguments(MiniYaml yaml)
		{
			var arguments = new Dictionary<string, object>();
			var argumentsNode = yaml.NodeWithKeyOrDefault("Arguments");
			if (argumentsNode != null)
			{
				foreach (var argumentNode in argumentsNode.Value.Nodes)
				{
					var argument = FieldLoader.Load<FluentArgument>(argumentNode.Value);
					if (argument.Type == FluentArgument.FluentArgumentType.Number)
					{
						if (!double.TryParse(argument.Value, out var number))
							Log.Write("debug", $"Failed to parse {argument.Value}");

						arguments.Add(argument.Key, number);
					}
					else
						arguments.Add(argument.Key, argument.Value);
				}
			}

			return arguments;
		}

		public LocalizedMessage(MiniYaml yaml)
		{
			// Let the FieldLoader do the dirty work of loading the public fields.
			FieldLoader.Load(this, yaml);
		}

		public static string Serialize(string key, Dictionary<string, object> arguments = null)
		{
			var root = new List<MiniYamlNode>
			{
				new("Protocol", ProtocolVersion.ToStringInvariant()),
				new("Key", key)
			};

			if (arguments != null)
			{
				var argumentsNode = new MiniYaml("", arguments
					.Select(a => new FluentArgument(a.Key, a.Value))
					.Select((argument, i) => new MiniYamlNode("Argument@" + i, FieldSaver.Save(argument))));

				root.Add(new MiniYamlNode("Arguments", argumentsNode));
			}

			return new MiniYaml("", root)
				.ToLines("LocalizedMessage")
				.JoinWith("\n");
		}
	}
}
