using SiraUtil.Affinity;

namespace SoloParty.AffinityPatches;

internal class GameEnergyCounterPatches : IAffinity
{
	public static bool GameStarted;
	public static bool EnergyDidReach0;
	public static int NotesPassed;
	public static int NotesCount;

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
		NotesCount = 0;
	}

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.HandleNoteWasCut)
	)]
	private void HandleNoteWasCutPatch(NoteController noteController)
	{
		if (noteController.noteData.gameplayType == NoteData.GameplayType.Bomb)
			return;
		NotesCount++;
		if (!EnergyDidReach0)
			NotesPassed++;
	}

	[AffinityPostfix]
	[AffinityPatch(
		typeof(GameEnergyCounter),
		nameof(GameEnergyCounter.HandleNoteWasMissed)
	)]
	private void HandleNoteWasMissedPatch(NoteController noteController)
	{
		if (noteController.noteData.gameplayType == NoteData.GameplayType.Bomb)
			return;
		NotesCount++;
		if (!EnergyDidReach0)
			NotesPassed++;
	}

	private static void OnEnergyDidReach0()
	{
		if (EnergyDidReach0)
			return;
		EnergyDidReach0 = true;
	}
}
