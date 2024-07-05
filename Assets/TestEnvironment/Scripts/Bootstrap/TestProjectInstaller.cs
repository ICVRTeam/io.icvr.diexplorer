using TestEnvironment.Services;
using TestEnvironment.Signals;
using Zenject;

namespace TestEnvironment.Bootstrap
{
    public class TestProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<DebugLogService>().AsSingle();
            Container.BindInterfacesTo<ProjectGreeter>().AsSingle();
            SignalBusInstaller.Install(Container);
            
            InstallSignals();
        }
        
        private void InstallSignals()
        {
            Container.DeclareSignal<ProjectContextSignal>();
            Container.DeclareSignal<UserJoinedSignal>();
            
            Container.BindSignal<UserJoinedSignal>()
                .ToMethod<Greeter3>(x => x.SayHello).FromResolve();

            Container.BindInterfacesTo<Greeter1>().AsSingle();
            Container.BindInterfacesTo<Greeter2>().AsSingle();
            Container.Bind<Greeter3>().AsSingle();
            Container.BindInterfacesTo<GameInitializer>().AsSingle();
        }
    }
}