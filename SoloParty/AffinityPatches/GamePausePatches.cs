using SiraUtil.Affinity;

namespace SoloParty.AffinityPatches;

internal sealed class GamePausePatches : IAffinity
{
	public static int Pauses;

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.Start)
	)]
	private void StartPatch()
	{
		Pauses = 0;
	}

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GamePause),
		nameof(GamePause.Pause)
	)]
	private void PausePatch()
	{
		Pauses++;
	}
}
