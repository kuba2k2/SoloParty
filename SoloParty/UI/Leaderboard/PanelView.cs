using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace SoloParty.UI.Leaderboard;

[HotReload(RelativePathToLayout = "PanelView.bsml")]
[ViewDefinition(nameof(SoloParty) + ".UI.Leaderboard.PanelView.bsml")]
public class PanelView : BSMLAutomaticViewController
{
	[UIAction("#post-parse")]
	private void PostParse()
	{
	}
}
