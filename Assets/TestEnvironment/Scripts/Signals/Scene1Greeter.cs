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

using UnityEngine;
using Zenject;

namespace TestEnvironment.Signals
{
    public class Scene1Greeter : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        public void SayHello(Scene1ContextSignal signal)
        {
            Debug.Log("Hello " + signal.Username + "!");
        }

        public void Awake()
        {
            _signalBus.Subscribe<Scene1ContextSignal>(SayHello);
        }
    }
}