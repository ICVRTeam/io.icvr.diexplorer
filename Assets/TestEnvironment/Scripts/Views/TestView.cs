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

using DiExplorer.Services;
using TestEnvironment.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestEnvironment.Views
{
    public class TestView : MonoBehaviour
    {
        [SerializeField] private Button _showAllBindingsButton; 
        [SerializeField] private Button _showAllInstancesButton;
        [SerializeField] private Button _fireSignalButton;
        
        private SignalBus _signalBus;
        
        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void Awake()
        {
            
            _fireSignalButton.OnClickAsObservable().Subscribe(_ =>
            {
                _signalBus.Fire(new UserJoinedSignal() { Username = "Test Fire Signal" });
            }).AddTo(gameObject);
        }
    }
}