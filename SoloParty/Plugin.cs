using HarmonyLib;
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
		zenjector.Install<MenuInstaller>(
			Location.Menu
		);
	}
}
