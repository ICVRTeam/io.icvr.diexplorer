// ICVR CONFIDENTIAL
// __________________
// 
// [2016] -  [2024] ICVR LLC
// All Rights Reserved.
// 
// NOTICE:  All information contained herein is, and remains
// the property of ICVR LLC and its suppliers,
// if any.  The intellectual and technical concepts contained
// here in are proprietary to ICVR LLC and its suppliers and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from ICVR LLC.

using DiExplorer.Containers;
using DiExplorer.Rules;
using DiExplorer.Scripts.Bootstrap;

namespace DiExplorer.Bootstrap
{
    internal class DiExplorerPlayInstaller : DiExplorerBaseInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.Bind<AbstractContainer>().To<ProjectContainer>().AsSingle();
            Container.Bind<AbstractContainer>().To<SceneContainer>().AsSingle();
            
            Container.BindInterfacesTo<DiExplorerInitializeRule>().AsSingle().NonLazy();
        }
    }
}