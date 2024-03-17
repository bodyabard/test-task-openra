using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class IngameMedicavStatsLogic : ChromeLogic
	{
		readonly World world;
		readonly Player player;
		readonly PlayerStatistics playerStatistics;

		private readonly LabelWidget healedLabel;
		private readonly LabelWidget transportedLabel;

		int nextCashTickTime = 0;
		int displayResources;

		[ObjectCreator.UseCtor]
		public IngameMedicavStatsLogic(Widget widget, ModData modData, World world)
		{
			this.world = world;
			player = world.LocalPlayer;

			healedLabel = widget.Get<LabelWidget>("HEALED_LABEL");
			healedLabel.GetText = () => "Amount of units healed: {0}";

			transportedLabel = widget.Get<LabelWidget>("TRANSPORTED_LABEL");
			transportedLabel.GetText = () => "Amount of units transported: {0}";

			playerStatistics = player.PlayerActor.Trait<PlayerStatistics>();
		}

		public override void Tick()
		{
			healedLabel.GetText = () => $"Amount of units healed: {playerStatistics.HealedByMedivac}";
			transportedLabel.GetText = () => "Amount of units transported: {0}";
		}
	}
}
