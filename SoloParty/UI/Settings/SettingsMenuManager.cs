using System;
using BeatSaberMarkupLanguage.Settings;
using Zenject;

namespace SoloParty.UI.Settings;

internal class SettingsMenuManager(
	SettingsMenu settingsMenu,
	BSMLSettings bsmlSettings
) : IInitializable, IDisposable
{
	private const string MenuName = nameof(SoloParty);
	private const string ResourcePath = nameof(SoloParty) + ".UI.Settings.settings.bsml";

	public void Initialize()
	{
		bsmlSettings.AddSettingsMenu(MenuName, ResourcePath, settingsMenu);
	}

	public void Dispose()
	{
		bsmlSettings.RemoveSettingsMenu(settingsMenu);
	}
}
