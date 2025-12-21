using System;
using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using Zenject;

namespace SoloParty.UI.Leaderboard;

internal class SoloLeaderboard(
	CustomLeaderboardManager manager,
	PanelView panelView,
	LeaderboardView leaderboardView,
	PluginConfig config
) : CustomLeaderboard, IInitializable, IDisposable
{
	protected override ViewController panelViewController => panelView;
	protected override ViewController leaderboardViewController => leaderboardView;
	protected override string leaderboardId => "SoloParty";

	public void Initialize()
	{
		manager.Register(this);
	}

	public void Dispose()
	{
		manager.Unregister(this);
	}

	public override bool ShowForLevel(BeatmapKey? beatmapKey)
	{
		return config.ModEnabled;
	}
}
