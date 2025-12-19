using HarmonyLib;
using Hive.Versioning;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using SoloParty.Installers;
using IpaLogger = IPA.Logging.Logger;
using IpaConfig = IPA.Config.Config;

namespace SoloParty;

[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
internal class Plugin
{
	internal static IpaLogger Log { get; private set; } = null!;
	internal static bool LeaderboardCoreInstalled { get; private set; }
	internal static bool SongPlayHistoryInstalled { get; private set; }

	[Init]
	public Plugin(IpaLogger ipaLogger, IpaConfig ipaConfig, Zenjector zenjector, PluginMetadata pluginMetadata)
	{
		Log = ipaLogger;
		zenjector.UseLogger(ipaLogger);
		zenjector.Install<AppInstaller>(
			Location.App,
			ipaConfig.Generated<PluginConfig>(),
			pluginMetadata,
			new Harmony(pluginMetadata.Id)
		);
		zenjector.Install<MenuInstaller>(Location.Menu);
		zenjector.Install<GameInstaller>(Location.StandardPlayer);
	}

	[OnStart]
	public void OnStart()
	{
		var leaderboardCore = PluginManager.GetPluginFromId("LeaderboardCore")?.HVersion;
		var songPlayHistory = PluginManager.GetPluginFromId("SongPlayHistory")?.HVersion;
		LeaderboardCoreInstalled = leaderboardCore != null && leaderboardCore >= new Version(1, 7, 0);
		SongPlayHistoryInstalled = songPlayHistory != null && songPlayHistory >= new Version(2, 2, 0);
	}
}
