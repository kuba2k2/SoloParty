using SiraUtil.Affinity;

namespace SoloParty.AffinityPatches;

internal sealed class RankModelPatches : IAffinity
{
	public static int MultipliedScore;
	public static int ModifiedScore;
	public static int MaxMultipliedScore;
	public static int MaxModifiedScore;

	[AffinityPostfix]
	[AffinityPatch(
		typeof(RankModel),
		nameof(RankModel.GetRankForScore)
	)]
	private void GetRankForScore(
		int multipliedScore,
		int modifiedScore,
		int maxMultipliedScore,
		int maxModifiedScore
	)
	{
		MultipliedScore = multipliedScore;
		ModifiedScore = modifiedScore;
		MaxMultipliedScore = maxMultipliedScore;
		MaxModifiedScore = maxModifiedScore;
	}
}
