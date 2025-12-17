namespace SoloParty.Utils;

public static class BeatmapKeyUtils
{
	public static string ToBeatmapKeyString(this BeatmapKey beatmapKey)
	{
		var levelId = beatmapKey.levelId;
		var difficulty = beatmapKey.difficulty switch
		{
			BeatmapDifficulty.Easy => 0,
			BeatmapDifficulty.Normal => 1,
			BeatmapDifficulty.Hard => 2,
			BeatmapDifficulty.Expert => 3,
			BeatmapDifficulty.ExpertPlus => 4,
			_ => (int)beatmapKey.difficulty
		};
		var characteristic = beatmapKey.beatmapCharacteristic.serializedName;

		return $"{levelId}___{difficulty}___{characteristic}";
	}
}
