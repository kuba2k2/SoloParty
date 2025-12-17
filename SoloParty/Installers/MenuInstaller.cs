using SiraUtil.Logging;
using SoloParty.AffinityPatches;
using SoloParty.Data.HighScore;
using SoloParty.UI.Settings;
using Zenject;

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
		Container.BindInterfacesTo<RankModelPatches>().AsSingle();

		if (Plugin.SongPlayHistoryInstalled)
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
