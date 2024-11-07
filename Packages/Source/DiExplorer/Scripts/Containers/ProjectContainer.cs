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

using DiExplorer.Storages;
using Zenject;

namespace DiExplorer.Containers
{
    internal class ProjectContainer : AbstractContainer
    {
        private const string ContextName = "Project";

        public override string ContainerName => GetContainerName();

        public ProjectContainer(
            SignalBus signalBusInstance,
            InheritorsStorage inheritorsStorage)
            : base(signalBusInstance, inheritorsStorage) { }

        protected override string GetContainerName()
        {
            return ContextName;
        }

        protected override Context GetContext()
        {
            var projectContext = ProjectContext.Instance;
            
            return projectContext != null ? projectContext : null;
        }
    }
}