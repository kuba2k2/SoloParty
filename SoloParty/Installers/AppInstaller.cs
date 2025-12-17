using HarmonyLib;
using IPA.Loader;
using SoloParty.Data.Record;
using Zenject;

namespace SoloParty.Installers;

internal class AppInstaller(
	PluginConfig config,
	PluginMetadata metadata,
	Harmony harmony
) : Installer
{
	public override void InstallBindings()
	{
		Container.BindInstance(config).AsSingle();
		Container.BindInstance(metadata).AsSingle();
		Container.BindInstance(harmony).AsSingle();
		Container.BindInterfacesAndSelfTo<SoloRecordManager>().AsSingle();
	}
}
