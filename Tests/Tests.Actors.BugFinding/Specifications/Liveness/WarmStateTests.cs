﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.Tests.Common.Events;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Coyote.Actors.BugFinding.Tests.Specifications
{
    public class WarmStateTests : BaseActorBugFindingTest
    {
        public WarmStateTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private class UserEvent : Event
        {
        }

        private class Done : Event
        {
        }

        private class Waiting : Event
        {
        }

        private class Computing : Event
        {
        }

        private class EventHandler : StateMachine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            [OnEventGotoState(typeof(UnitEvent), typeof(WaitForUser))]
            private class Init : State
            {
            }

            private void InitOnEntry() => this.RaiseEvent(UnitEvent.Instance);

            [OnEntry(nameof(WaitForUserOnEntry))]
            [OnEventGotoState(typeof(UserEvent), typeof(HandleEvent))]
            private class WaitForUser : State
            {
            }

            private void WaitForUserOnEntry()
            {
                this.Monitor<WatchDog>(new Waiting());
                this.SendEvent(this.Id, new UserEvent());
            }

            [OnEntry(nameof(HandleEventOnEntry))]
            private class HandleEvent : State
            {
            }

            private void HandleEventOnEntry()
            {
                this.Monitor<WatchDog>(new Computing());
            }
        }

        private class WatchDog : Monitor
        {
            [Start]
            [OnEventGotoState(typeof(Waiting), typeof(CanGetUserInput))]
            [OnEventGotoState(typeof(Computing), typeof(CannotGetUserInput))]
            private class CanGetUserInput : State
            {
            }

            [OnEventGotoState(typeof(Waiting), typeof(CanGetUserInput))]
            [OnEventGotoState(typeof(Computing), typeof(CannotGetUserInput))]
            private class CannotGetUserInput : State
            {
            }
        }

        [Fact(Timeout = 5000)]
        public void TestWarmState()
        {
            this.Test(r =>
            {
                r.RegisterMonitor<WatchDog>();
                r.CreateActor(typeof(EventHandler));
            },
            configuration: this.GetConfiguration().WithDFSStrategy());
        }
    }
}
