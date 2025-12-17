using Hive.Versioning;
using IPA.Loader;
using SiraUtil.Logging;
using SoloParty.AffinityPatches;
using SoloParty.Data;
using SoloParty.Data.HighScore;
using Zenject;
using SoloParty.UI;

namespace SoloParty.Installers;

internal class MenuInstaller(
	SiraLog log
) : Installer
{
	public override void InstallBindings()
	{
		Container.Bind<SettingsMenu>().AsSingle();
		Container.BindInterfacesTo<SettingsMenuManager>().AsSingle();

		Container.BindInterfacesTo<SoloFreePlayFlowCoordinatorPatches>().AsSingle();
		Container.BindInterfacesTo<EnterPlayerGuestNameViewControllerPatches>().AsSingle();

		var sph = PluginManager.GetPluginFromId("SongPlayHistory");
		if (sph != null && sph.HVersion >= new Version(2, 2, 0))
		{
			log.Info("SongPlayHistory found, enabling high score setter and UI patches");
			Container.BindInterfacesTo<SongPlayHistoryHighScoreSetter>().AsSingle();
			Container.BindInterfacesAndSelfTo<SongPlayHistoryUIPatches>().AsSingle();
		}
		else
		{
			log.Warn("SongPlayHistory NOT found, disabling high score setter");
			Container.BindInterfacesTo<DummyHighScoreSetter>().AsSingle();
		}
	}
}
