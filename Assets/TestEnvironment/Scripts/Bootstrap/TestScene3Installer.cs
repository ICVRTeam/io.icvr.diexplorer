using TestEnvironment.Interfaces;
using TestEnvironment.Models;
using Zenject;

namespace TestEnvironment.Bootstrap
{
    public class TestScene3Installer : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInformant>().To<InformantScene3>().AsSingle();
        }
    }
}
