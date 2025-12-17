namespace SoloParty.Data.HighScore;

internal interface IHighScoreSetter
{
	public void UpdateHighScore(LevelCompletionResults results, int modifiedScore);
}
