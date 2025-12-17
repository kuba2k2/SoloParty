using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace SoloParty.UI;

internal class SettingsMenu(
	PluginConfig config
)
{
	public bool ModEnabled
	{
		get => config.ModEnabled;
		set => config.ModEnabled = value;
	}

	public bool SoloChooserEnabled
	{
		get => config.SoloChooserEnabled;
		set => config.SoloChooserEnabled = value;
	}

	public bool PlayerNameAutoAccept
	{
		get => config.PlayerNameAutoAccept;
		set => config.PlayerNameAutoAccept = value;
	}

	public int PlayerNameMaxCount
	{
		get => config.PlayerNameMaxCount;
		set => config.PlayerNameMaxCount = value;
	}

	public bool ResultsHighScoreFireworks
	{
		get => config.ResultsHighScoreFireworks;
		set => config.ResultsHighScoreFireworks = value;
	}

	public bool ResultsHighScoreSetter
	{
		get => config.ResultsHighScoreSetter;
		set => config.ResultsHighScoreSetter = value;
	}

	public bool LeaderboardSongPlayHistory
	{
		get => config.LeaderboardSongPlayHistory;
		set => config.LeaderboardSongPlayHistory = value;
	}

	[UIComponent("ModEnabled")] private readonly ToggleSetting _modEnabled = null!;
	[UIComponent("SoloChooser")] private readonly ToggleSetting _soloChooser = null!;
	[UIComponent("NameAutoAccept")] private readonly ToggleSetting _nameAutoAccept = null!;
	[UIComponent("NameCount")] private readonly IncrementSetting _nameCount = null!;
	[UIComponent("HighScoreFireworks")] private readonly ToggleSetting _highScoreFireworks = null!;
	[UIComponent("HighScoreSetter")] private readonly ToggleSetting _highScoreSetter = null!;
	[UIComponent("LeaderboardSongPlay")] private readonly ToggleSetting _leaderboardSongPlay = null!;

	[UIAction("#post-parse")]
	private void PostParse()
	{
		UpdateUI();
	}

	[UIAction("UpdateUI")]
	private void UpdateUI(object? _ = null)
	{
		_soloChooser.Interactable = _modEnabled.Value;
		_nameAutoAccept.Interactable = _modEnabled.Value;
		_nameCount.Interactable = _modEnabled.Value;
		_highScoreFireworks.Interactable = _modEnabled.Value && _soloChooser.Value;
		_highScoreSetter.Interactable = _modEnabled.Value && _soloChooser.Value && Plugin.SongPlayHistoryInstalled;
		_leaderboardSongPlay.Interactable = _modEnabled.Value && Plugin.SongPlayHistoryInstalled;
	}
}
