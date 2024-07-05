using TestEnvironment.Interfaces;
using TestEnvironment.Models;
using Zenject;

namespace TestEnvironment.Bootstrap
{
    public class TestScene2Installer : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInformant>().To<InformantScene2>().AsSingle();
        }
    }
}
