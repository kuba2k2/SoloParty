using SiraUtil.Affinity;

namespace SoloParty.AffinityPatches;

internal class GameEnergyCounterPatches : IAffinity
{
	public static bool GameStarted;
	public static bool EnergyDidReach0;
	public static int NotesPassed;

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.Start)
	)]
	private void StartPatch(GameEnergyCounter __instance)
	{
		__instance.gameEnergyDidReach0Event -= OnEnergyDidReach0;
		__instance.gameEnergyDidReach0Event += OnEnergyDidReach0;
		GameStarted = true;
		EnergyDidReach0 = false;
		NotesPassed = 0;
	}

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.HandleNoteWasCut)
	)]
	private void HandleNoteWasCutPatch(NoteController noteController)
	{
		if (EnergyDidReach0 || noteController.noteData.gameplayType == NoteData.GameplayType.Bomb)
			return;
		NotesPassed++;
	}

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.HandleNoteWasMissed)
	)]
	private void HandleNoteWasMissedPatch(NoteController noteController)
	{
		if (EnergyDidReach0 || noteController.noteData.gameplayType == NoteData.GameplayType.Bomb)
			return;
		NotesPassed++;
	}

	private static void OnEnergyDidReach0()
	{
		if (EnergyDidReach0)
			return;
		EnergyDidReach0 = true;
	}
}
