using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using SoloParty.Data.Manager;
using TMPro;

namespace SoloParty.UI.Settings;

internal class SettingsMenu(
	PluginConfig config,
	ExternalImportManager importManager
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
	[UIComponent("LeaderboardBSD")] private readonly ToggleSetting _leaderboardBeatSavior = null!;
	[UIComponent("HighScoreFireworks")] private readonly ToggleSetting _highScoreFireworks = null!;
	[UIComponent("HighScoreSetter")] private readonly ToggleSetting _highScoreSetter = null!;
	[UIComponent("NameAutoAccept")] private readonly ToggleSetting _nameAutoAccept = null!;
	[UIComponent("NameCount")] private readonly IncrementSetting _nameCount = null!;
	[UIComponent("ImportTitle")] private readonly TextMeshProUGUI _importTitle = null!;
	[UIComponent("ImportMessage")] private readonly TextMeshProUGUI _importMessage = null!;
	[UIComponent("ImportPlayerName")] private readonly StringSetting _importPlayerName = null!;
	[UIComponent("SuccessMessage")] private readonly TextMeshProUGUI _successMessage = null!;
	[UIParams] private readonly BSMLParserParams _parserParams = null!;

	private const string ImportMessagePrefix =
		"<br>No data will be lost, however this process is not reversible.<br>";

	private const string ImportMessageSuffix =
		"<br><color=#ababab><size=70%>" +
		"Depending on the amount of data being imported, this can take a few minutes.<br>" +
		"The game might appear frozen and unresponsive." +
		"</size></color>";

	private string _importProviderName = "";

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
		_leaderboardBeatSavior.Interactable = false;
		_highScoreFireworks.Interactable = enabled && _soloTracking.Value;
		_highScoreSetter.Interactable = enabled && _soloTracking.Value && Plugin.SongPlayHistoryInstalled;
		_nameAutoAccept.Interactable = enabled;
		_nameCount.Interactable = enabled;
	}

	[UIAction("OnImportPartyClick")]
	private void OnImportPartyClick()
	{
		_importProviderName = "PartyLeaderboard";
		_importTitle.text = "Import from Party mode";
		_importMessage.text =
			"This will copy all scores from Party mode into SoloParty's internal database." +
			ImportMessagePrefix +
			"<color=red> </color>" +
			ImportMessageSuffix;
		_importPlayerName.gameObject.SetActive(false);
		_importPlayerName.Text = "";
		_parserParams.EmitEvent("show-import");
	}

	[UIAction("OnImportSongPlayHistoryClick")]
	private void OnImportSongPlayHistoryClick()
	{
		_importProviderName = "SongPlayHistory";
		_importTitle.text = "Import from SongPlayHistory";
		_importMessage.text =
			"This will copy all scores from SongPlayHistory into SoloParty's internal database." +
			ImportMessagePrefix +
			"<color=red>Make sure to import Party leaderboard first!</color>" +
			ImportMessageSuffix;
		_importPlayerName.gameObject.SetActive(true);
		_importPlayerName.Text = "";
		_parserParams.EmitEvent("show-import");
	}

	[UIAction("OnImportBeatSaviorDataClick")]
	private void OnImportBeatSaviorDataClick()
	{
		_importProviderName = "BeatSaviorData";
		_importTitle.text = "Import from BeatSaviorData";
		_importMessage.text =
			"This will copy all scores from BeatSaviorData into SoloParty's internal database." +
			ImportMessagePrefix +
			"<color=red>Make sure to import SongPlayHistory first!</color>" +
			ImportMessageSuffix;
		_importPlayerName.gameObject.SetActive(true);
		_importPlayerName.Text = "";
		_parserParams.EmitEvent("show-import");
	}

	[UIAction("OnImportStartClick")]
	private void OnImportStartClick()
	{
		_parserParams.EmitEvent("hide-import");

		var importer = importManager.GetByName(_importProviderName);
		if (importer == null)
			return;
		var playerName = _importPlayerName.Text;
		if (playerName == "")
			playerName = null;

		_parserParams.EmitEvent("show-progress");
		importManager
			.ImportRecords(importer, playerName)
			.ContinueWith(async task =>
			{
				await UnityGame.SwitchToMainThreadAsync();
				_parserParams.EmitEvent("hide-progress");
				_successMessage.text = task.IsFaulted
					? $"<color=#ff0000>Import failed: {task.Exception}</color>"
					: $"{task.Result.AddCount} record(s) added.<br>" +
					  $"{task.Result.MergeCount} record(s) updated.<br>" +
					  $"{task.Result.SameCount} record(s) unchanged.";
				_parserParams.EmitEvent("show-success");
			});
	}
}
