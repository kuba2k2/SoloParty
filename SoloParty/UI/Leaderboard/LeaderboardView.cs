using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using LeaderboardCore.Interfaces;
using SiraUtil.Logging;
using SoloParty.Data.Manager;
using SoloParty.Data.Models;
using SoloParty.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SoloParty.UI.Leaderboard;

[HotReload(RelativePathToLayout = "LeaderboardView.bsml")]
[ViewDefinition(nameof(SoloParty) + ".UI.Leaderboard.LeaderboardView.bsml")]
internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet
{
	[Inject] private readonly SiraLog _log = null!;
	[Inject] private readonly PluginConfig _config = null!;
	[Inject] private readonly ExternalRecordManager _recordManager = null!;

	[UIComponent("pageUp")] private readonly Button _pageUp = null!;
	[UIComponent("pageDown")] private readonly Button _pageDown = null!;
	[UIComponent("sortLastPlayedIcon")] private readonly Image _sortLastPlayedIcon = null!;
	[UIComponent("sortBestScoreIcon")] private readonly Image _sortBestScoreIcon = null!;
	[UIComponent("sortGoodCutsIcon")] private readonly Image _sortGoodCutsIcon = null!;
	[UIComponent("sortControl")] private readonly SegmentedControl _sortControl = null!;
	[UIComponent("noScores")] private readonly TextMeshProUGUI _noScores = null!;
	[UIComponent("leaderboard")] private readonly LeaderboardTableView _leaderboardTable = null!;
	[UIComponent("leaderboard")] private readonly Transform _leaderboardTransform = null!;

	[UIValue("sortIcons")]
	private List<IconSegmentedControl.DataItem> sortIcons =>
	[
		new(_sortLastPlayedIcon.sprite, "Last Played"),
		new(_sortBestScoreIcon.sprite, "Best Score"),
		new(_sortGoodCutsIcon.sprite, "Good Cuts"),
	];

	private BeatmapKey _beatmapKey;
	private List<SoloRecord> _allRecords = [];
	private int _offset;

	[UIAction("#post-parse")]
	private void PostParse()
	{
		var loadingControl = _leaderboardTransform.Find("LoadingControl").gameObject;
		var loadingContainer = loadingControl.transform.Find("LoadingContainer");
		loadingContainer.gameObject.SetActive(false);
		Destroy(loadingContainer.Find("Text").gameObject);
		Destroy(loadingControl.transform.Find("RefreshContainer").gameObject);
		Destroy(loadingControl.transform.Find("DownloadingContainer").gameObject);
		OnLeaderboardSet(_beatmapKey);
	}

	public void OnLeaderboardSet(BeatmapKey beatmapKey)
	{
		if (!isActivated || !beatmapKey.IsValid())
			return;
		_log.Info($"Leaderboard set: {beatmapKey.ToBeatmapKeyString()}");
		_beatmapKey = beatmapKey;
		_allRecords = _recordManager.GetRecords(beatmapKey);
		_offset = 0;
		ShowLeaderboard();
	}

	public void OnPageUpClick()
	{
		if (_offset <= 0)
			return;
		_offset = Math.Max(_offset - 10, 0);
		ShowLeaderboard();
	}

	public void OnPageDownClick()
	{
		if (_offset + 10 >= _allRecords.Count)
			return;
		_offset = Math.Min(_offset + 10, _allRecords.Count);
		ShowLeaderboard();
	}

	public void OnSortClick(SegmentedControl control, int index)
	{
		var sortType = (SortType)index;
		if (_config.LeaderboardSortType == sortType)
			return;
		_config.LeaderboardSortType = sortType;
		ShowLeaderboard();
	}

	private void ShowLeaderboard()
	{
		_pageUp.interactable = _offset > 0;
		_pageDown.interactable = _offset + 10 < _allRecords.Count;
		_sortControl.SelectCellWithNumber((int)_config.LeaderboardSortType);

		_noScores.gameObject.SetActive(_allRecords.Count == 0);
		if (_allRecords.Count == 0)
		{
			_leaderboardTable.SetScores([], -1);
			return;
		}

		var records = _allRecords
			.OrderByDescending(record =>
				_config.LeaderboardSortType switch
				{
					SortType.LastPlayed => record.Date,
					SortType.BestScore => record.ModifiedScore,
					SortType.GoodCuts => record.GoodCutsCount,
					_ => record.ModifiedScore
				}
			)
			.Skip(_offset)
			.Take(10)
			.ToList();

		var scores = records
			.Select((record, index) => new LeaderboardTableView.ScoreData(
				score: record.ModifiedScore,
				playerName: record.PlayerName,
				rank: _offset + index + 1,
				fullCombo: record.EndState == EndState.FullCombo
			))
			.ToList();

		_leaderboardTable.SetScores(scores, -1);
	}
}
