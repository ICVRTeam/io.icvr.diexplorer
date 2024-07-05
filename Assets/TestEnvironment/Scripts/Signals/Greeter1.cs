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

using System;
using UnityEngine;
using Zenject;

namespace TestEnvironment.Signals
{
    public class Greeter1 : IInitializable, IDisposable
    {
        readonly SignalBus _signalBus;

        public Greeter1(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UserJoinedSignal>(OnUserJoined);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<UserJoinedSignal>(OnUserJoined);
        }

        void OnUserJoined(UserJoinedSignal args)
        {
            SayHello(args.Username);
        }

        public void SayHello(string userName)
        {
            Debug.Log("Hello " + userName + "!");
        }
    }
}