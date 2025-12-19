using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using SoloParty.Data.Models;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace SoloParty;

internal class PluginConfig
{
	private bool _leaderboardSongPlayHistory = true;

	public virtual bool ModEnabled { get; set; } = true;
	public virtual bool SoloTrackingEnabled { get; set; } = true;
	public virtual bool SoloChooserEnabled { get; set; } = true;
	public virtual bool PlayerNameAutoAccept { get; set; } = false;
	public virtual int PlayerNameMaxCount { get; set; } = 5;
	public virtual bool PlayerNameForceSolo { get; set; } = false;
	public virtual List<string>? PlayerNameForceListSolo { get; set; } = null;
	public virtual bool ResultsHighScoreFireworks { get; set; } = true;
	public virtual bool ResultsHighScoreSetter { get; set; } = true;
	public virtual SortType LeaderboardSortType { get; set; } = SortType.LastPlayed;

	public virtual bool LeaderboardSongPlayHistory
	{
		get => _leaderboardSongPlayHistory;
		set
		{
			_leaderboardSongPlayHistory = value;
			LeaderboardSongPlayHistoryChanged?.Invoke();
		}
	}

	public event Action? LeaderboardSongPlayHistoryChanged;
}
