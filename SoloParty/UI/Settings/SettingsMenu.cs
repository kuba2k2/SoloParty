using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace SoloParty.UI.Settings;

internal class SettingsMenu(
	PluginConfig config
)
{
	public bool ModEnabled
	{
		get => config.ModEnabled;
		set => config.ModEnabled = value;
	}

	public bool SoloTrackingEnabled
	{
		get => config.SoloTrackingEnabled;
		set => config.SoloTrackingEnabled = value;
	}

	public bool SoloChooserEnabled
	{
		get => config.SoloChooserEnabled;
		set => config.SoloChooserEnabled = value;
	}

	public bool LeaderboardPartyModeRecords
	{
		get => config.LeaderboardPartyModeRecords;
		set => config.LeaderboardPartyModeRecords = value;
	}

	public bool LeaderboardSongPlayHistoryRecords
	{
		get => config.LeaderboardSongPlayHistoryRecords;
		set => config.LeaderboardSongPlayHistoryRecords = value;
	}

	public bool LeaderboardSongPlayHistoryName
	{
		get => config.LeaderboardSongPlayHistoryName;
		set => config.LeaderboardSongPlayHistoryName = value;
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

	[UIComponent("ModEnabled")] private readonly ToggleSetting _modEnabled = null!;
	[UIComponent("SoloTracking")] private readonly ToggleSetting _soloTracking = null!;
	[UIComponent("SoloChooser")] private readonly ToggleSetting _soloChooser = null!;
	[UIComponent("LeaderboardParty")] private readonly ToggleSetting _leaderboardParty = null!;
	[UIComponent("LeaderboardSPH")] private readonly ToggleSetting _leaderboardSongPlay = null!;
	[UIComponent("LeaderboardSPHName")] private readonly ToggleSetting _leaderboardSongPlayName = null!;
	[UIComponent("HighScoreFireworks")] private readonly ToggleSetting _highScoreFireworks = null!;
	[UIComponent("HighScoreSetter")] private readonly ToggleSetting _highScoreSetter = null!;
	[UIComponent("NameAutoAccept")] private readonly ToggleSetting _nameAutoAccept = null!;
	[UIComponent("NameCount")] private readonly IncrementSetting _nameCount = null!;

	[UIAction("#post-parse")]
	private void PostParse()
	{
		UpdateUI();
	}

	[UIAction("UpdateUI")]
	private void UpdateUI(object? _ = null)
	{
		var enabled = _modEnabled.Value;
		_soloTracking.Interactable = enabled;
		_soloChooser.Interactable = enabled && _soloTracking.Value;
		_leaderboardParty.Interactable = enabled;
		_leaderboardSongPlay.Interactable = enabled && Plugin.SongPlayHistoryInstalled;
		_leaderboardSongPlayName.Interactable = enabled && Plugin.SongPlayHistoryInstalled;
		_highScoreFireworks.Interactable = enabled && _soloTracking.Value;
		_highScoreSetter.Interactable = enabled && _soloTracking.Value && Plugin.SongPlayHistoryInstalled;
		_nameAutoAccept.Interactable = enabled;
		_nameCount.Interactable = enabled;
	}
}
