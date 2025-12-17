using IPA.Utilities;
using SiraUtil.Logging;
using SongPlayHistory.SongPlayTracking;

namespace SoloParty.Data.HighScore;

internal class SongPlayHistoryHighScoreSetter(
	SiraLog log,
	ExtraCompletionDataManager extraCompletionDataManager
) : IHighScoreSetter
{
	public void UpdateHighScore(LevelCompletionResults results, int modifiedScore)
	{
		var extraData = extraCompletionDataManager.GetExtraData(results);
		if (extraData == null)
			return;

		if (extraData.PreviousPlayerLevelStats?.validScore != true)
			return;

		log.Info($"Updating SongPlayHistory previous high score to {modifiedScore}");
		extraData.PreviousPlayerLevelStats.SetField("_highScore", modifiedScore);
	}
}
