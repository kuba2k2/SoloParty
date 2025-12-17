using System;
using System.Collections.Generic;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;

namespace SoloParty.AffinityPatches;

internal sealed class EnterPlayerGuestNameViewControllerPatches(
	PluginConfig config,
	SiraLog log
) : IAffinity
{
	[AffinityPostfix]
	[AffinityPatch(
		typeof(EnterPlayerGuestNameViewController),
		nameof(EnterPlayerGuestNameViewController.DidActivate)
	)]
	private void DidActivatePatch(
		EnterPlayerGuestNameViewController __instance,
		bool firstActivation,
		bool addedToHierarchy,
		bool screenSystemEnabling
	)
	{
		if (!config.ModEnabled)
			return;
		if (!addedToHierarchy)
			return;
		log.Debug("Patching player name view");

		var isSolo = true; // TODO

		List<string> nameList = __instance._playerDataModel.playerData.guestPlayerNames;
		var nameCount = Mathf.Min(nameList.Count, config.PlayerNameMaxCount);

		if (isSolo && config is { PlayerNameForceSolo: true, PlayerNameForceListSolo.Count: > 0 })
		{
			log.Debug("- overriding player name list");
			nameList = config.PlayerNameForceListSolo;
			nameCount = nameList.Count;
		}

		log.Debug($"- adding total {nameCount} items, auto accept: {config.PlayerNameAutoAccept}");
		__instance._guestNameButtonsListItemsList.SetData(nameCount, (idx, item) =>
		{
			var guestPlayerName = nameList[idx];
			if (guestPlayerName.Length > 40)
				guestPlayerName = guestPlayerName[..40];

			item.nameText = guestPlayerName;
			item.buttonPressed = (Action)(() =>
			{
				__instance._nameInputFieldView.SetText(guestPlayerName);
				if (config.PlayerNameAutoAccept)
					__instance.OkButtonPressed();
			});
		});
	}
}
