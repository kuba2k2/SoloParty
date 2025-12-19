using SoloParty.AffinityPatches;
using Zenject;

namespace SoloParty.Installers;

internal class GameInstaller : Installer
{
	public override void InstallBindings()
	{
		Container.BindInterfacesTo<GameEnergyCounterPatches>().AsSingle();
	}
}
