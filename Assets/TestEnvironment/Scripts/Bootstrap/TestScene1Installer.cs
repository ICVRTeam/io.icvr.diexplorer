using TestEnvironment.Interfaces;
using TestEnvironment.Models;
using TestEnvironment.Signals;
using Zenject;

namespace TestEnvironment.Bootstrap
{
    public class TestScene1Installer : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInformant>().To<InformantScene1>().AsSingle();
            Container.DeclareSignal<Scene1ContextSignal>();
        }
    }
}
