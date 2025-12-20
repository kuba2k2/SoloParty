using System;
using System.Collections.Generic;
using System.Globalization;
using BeatSaberMarkupLanguage.Attributes;
using SoloParty.Data.Models;
using SoloParty.Utils;
using TMPro;
using UnityEngine.UI;

namespace SoloParty.UI.Leaderboard;

internal class LeaderboardViewRow
{
	[UIComponent("row")] private readonly LayoutGroup _row = null!;
	[UIComponent("background")] private readonly LayoutGroup _background = null!;
	[UIComponent("rank")] private readonly TextMeshProUGUI _rank = null!;
	[UIComponent("playerName")] private readonly TextMeshProUGUI _playerName = null!;
	[UIComponent("modifiers")] private readonly TextMeshProUGUI _modifiers = null!;
	[UIComponent("accuracy")] private readonly TextMeshProUGUI _accuracy = null!;
	[UIComponent("score")] private readonly TextMeshProUGUI _score = null!;
	[UIComponent("date")] private readonly TextMeshProUGUI _date = null!;
	[UIComponent("mistakes")] private readonly TextMeshProUGUI _mistakes = null!;
	[UIComponent("divider")] private readonly LayoutGroup _divider = null!;

	private readonly NumberFormatInfo _numberFormat = new()
	{
		NumberGroupSeparator = " ",
		NumberDecimalSeparator = ",",
		PercentPositivePattern = 1,
		PercentSymbol = ""
	};

	private readonly Dictionary<RankModel.Rank, string> _rankColorMap = new()
	{
		{ RankModel.Rank.SSS, "#00FFFF" },
		{ RankModel.Rank.SS, "#00FFFF" },
		{ RankModel.Rank.S, "#FFFFFF" },
		{ RankModel.Rank.A, "#00FF00" },
		{ RankModel.Rank.B, "#FFEB04" },
		{ RankModel.Rank.C, "#FF8000" },
		{ RankModel.Rank.D, "#FF0000" },
		{ RankModel.Rank.E, "#FF0000" }
	};

	private const string DimColor = "#ABABAB";
	private const string FullComboColor = "#88FF88";
	private const string NoMistakesColor = "#FFFFFF";
	private const string MistakesColor = "#FF8888";
	private const string FailedColor = "#FF3D00";

	public void SetRecord(int offset, int index, SoloRecord? record, bool isLast = false)
	{
		_row.gameObject.SetActive(record != null);
		if (record == null)
			return;
		_background.gameObject.SetActive(record.IsLatest);
		_rank.text = (offset + index + 1).ToString();
		_playerName.text = record.PlayerName ?? $"<color={DimColor}>No Name</color>";
		ShowModifiers(record);
		ShowAccuracy(record);
		ShowScore(record);
		ShowDate(record);
		ShowMistakes(record);
		_divider.gameObject.SetActive(!isLast);
	}

	private void ShowModifiers(SoloRecord record)
	{
		if (record.Modifiers == Modifier.None)
		{
			_modifiers.text = "";
			return;
		}

		var modifiers = record.Modifiers.ToModifierString();
		_modifiers.text = $"<color={DimColor}>{modifiers}</color>";
	}

	private void ShowAccuracy(SoloRecord record)
	{
		if (record.MaxModifiedScore <= 0 || record.MaxMultipliedScore < 0)
		{
			_accuracy.text = "";
			return;
		}

		var accuracy = record.Accuracy.ToString("P2", _numberFormat);
		var color = _rankColorMap.GetValueOrDefault(record.Rank);
		_accuracy.text = $"<color={color}>{accuracy}<size=70%>%</size></color>";
	}

	private void ShowScore(SoloRecord record)
	{
		if (record.ModifiedScore == -1)
		{
			_score.text = "";
			return;
		}

		var score = record.ModifiedScore.ToString("N0", _numberFormat);
		_score.text = $"{score}";
	}

	private void ShowDate(SoloRecord record)
	{
		var timeSpan = DateTime.Now - record.Date.ToLocalDateTime();

		var date = timeSpan.FormatTimeAgo();
		_date.text = $"{date}";
	}

	private void ShowMistakes(SoloRecord record)
	{
		string mistakes;
		string color;
		switch (record.EndState)
		{
			case EndState.FullCombo:
				mistakes = "FC";
				color = FullComboColor;
				break;

			case EndState.Cleared:
				if (record.BadCutsCount == -1 || record.MissedCount == -1)
					goto unknown;
				var mistakesCount = record.BadCutsCount + record.MissedCount;
				mistakes = $"{mistakesCount}x";
				color = mistakesCount == 0 ? NoMistakesColor : MistakesColor;
				break;

			case EndState.SoftFailed:
			case EndState.Failed:
				if (record.NotesCount == -1 || record.NotesPassed == -1)
					goto unknown;
				var notesLeft = record.NotesCount - record.NotesPassed;
				mistakes = notesLeft >= 100
					? $"<size=70%>+{notesLeft}</size>"
					: $"+{notesLeft}";
				color = FailedColor;
				break;

			case EndState.Unknown:
			default:
				unknown:
				_mistakes.text = "";
				return;
		}

		_mistakes.text = $"<color={color}>{mistakes}</color>";
	}
}
