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

using TestEnvironment.Factrories;
using TestEnvironment.Services;
using TestEnvironment.Views;
using UnityEngine;
using Zenject;

namespace TestEnvironment.Bootstrap
{
    public class GeneralTestSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _gameObject;
        
        public override void InstallBindings()
        {
            Container.Bind<TestBindingInstanceComponent>().FromNewComponentOnNewGameObject().WithGameObjectName("Object bind in installer").AsSingle().NonLazy();
            Container.Bind<LoadSceneService>().AsSingle();
            Container.BindFactory<TestFactoryInstance, TestFactoryInstance, TestInstanceFactory>();
        }
    }
}