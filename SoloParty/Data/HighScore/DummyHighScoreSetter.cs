using IPA.Utilities;
using SiraUtil.Logging;
using SongPlayHistory.SongPlayTracking;

namespace SoloParty.Data.HighScore;

internal class DummyHighScoreSetter : IHighScoreSetter
{
	public void UpdateHighScore(LevelCompletionResults results, int modifiedScore)
	{
	}
}
