# IActorRuntimeLog interface

Interface that allows an external module to track what is happening in the [`IActorRuntime`](./IActorRuntime.md).

```csharp
public interface IActorRuntimeLog
```

## Members

| name | description |
| --- | --- |
| [OnAssertionFailure](IActorRuntimeLog/OnAssertionFailure.md)(…) | Invoked when the specified assertion failure has occurred. |
| [OnCompleted](IActorRuntimeLog/OnCompleted.md)() | Invoked when a log is complete (and is about to be closed). |
| [OnCreateActor](IActorRuntimeLog/OnCreateActor.md)(…) | Invoked when the specified actor has been created. |
| [OnCreateMonitor](IActorRuntimeLog/OnCreateMonitor.md)(…) | Invoked when the specified monitor has been created. |
| [OnCreateStateMachine](IActorRuntimeLog/OnCreateStateMachine.md)(…) | Invoked when the specified state machine has been created. |
| [OnCreateTimer](IActorRuntimeLog/OnCreateTimer.md)(…) | Invoked when the specified actor timer has been created. |
| [OnDefaultEventHandler](IActorRuntimeLog/OnDefaultEventHandler.md)(…) | Invoked when the specified actor is idle (there is nothing to dequeue) and the default event handler is about to be executed. |
| [OnDequeueEvent](IActorRuntimeLog/OnDequeueEvent.md)(…) | Invoked when the specified event is dequeued by an actor. |
| [OnEnqueueEvent](IActorRuntimeLog/OnEnqueueEvent.md)(…) | Invoked when the specified event is about to be enqueued to an actor. |
| [OnEventHandlerTerminated](IActorRuntimeLog/OnEventHandlerTerminated.md)(…) | Invoked when the event handler of the specified actor terminated. |
| [OnExceptionHandled](IActorRuntimeLog/OnExceptionHandled.md)(…) | Invoked when the specified actor has handled a thrown exception. |
| [OnExceptionThrown](IActorRuntimeLog/OnExceptionThrown.md)(…) | Invoked when the specified actor throws an exception without handling it. |
| [OnExecuteAction](IActorRuntimeLog/OnExecuteAction.md)(…) | Invoked when the specified actor executes an action. |
| [OnGotoState](IActorRuntimeLog/OnGotoState.md)(…) | Invoked when the specified state machine performs a goto transition to the specified state. |
| [OnHalt](IActorRuntimeLog/OnHalt.md)(…) | Invoked when the specified actor has been halted. |
| [OnHandleRaisedEvent](IActorRuntimeLog/OnHandleRaisedEvent.md)(…) | Invoked when the specified actor handled a raised event. |
| [OnMonitorError](IActorRuntimeLog/OnMonitorError.md)(…) | Invoked when the specified monitor finds an error. |
| [OnMonitorExecuteAction](IActorRuntimeLog/OnMonitorExecuteAction.md)(…) | Invoked when the specified monitor executes an action. |
| [OnMonitorProcessEvent](IActorRuntimeLog/OnMonitorProcessEvent.md)(…) | Invoked when the specified monitor is about to process an event. |
| [OnMonitorRaiseEvent](IActorRuntimeLog/OnMonitorRaiseEvent.md)(…) | Invoked when the specified monitor raised an event. |
| [OnMonitorStateTransition](IActorRuntimeLog/OnMonitorStateTransition.md)(…) | Invoked when the specified monitor enters or exits a state. |
| [OnPopState](IActorRuntimeLog/OnPopState.md)(…) | Invoked when the specified state machine has popped its current state. |
| [OnPopStateUnhandledEvent](IActorRuntimeLog/OnPopStateUnhandledEvent.md)(…) | Invoked when the specified event cannot be handled in the current state, its exit handler is executed and then the state is popped and any previous "current state" is reentered. This handler is called when that pop has been done. |
| [OnPushState](IActorRuntimeLog/OnPushState.md)(…) | Invoked when the specified state machine is being pushed to a state. |
| [OnRaiseEvent](IActorRuntimeLog/OnRaiseEvent.md)(…) | Invoked when the specified state machine raises an event. |
| [OnRandom](IActorRuntimeLog/OnRandom.md)(…) | Invoked when the specified controlled nondeterministic result has been obtained. |
| [OnReceiveEvent](IActorRuntimeLog/OnReceiveEvent.md)(…) | Invoked when the specified event is received by an actor. |
| [OnSendEvent](IActorRuntimeLog/OnSendEvent.md)(…) | Invoked when the specified event is sent to a target actor. |
| [OnStateTransition](IActorRuntimeLog/OnStateTransition.md)(…) | Invoked when the specified state machine enters or exits a state. |
| [OnStopTimer](IActorRuntimeLog/OnStopTimer.md)(…) | Invoked when the specified actor timer has been stopped. |
| [OnWaitEvent](IActorRuntimeLog/OnWaitEvent.md)(…) | Invoked when the specified actor waits to receive an event of a specified type. (2 methods) |

## Remarks

See [Logging](/coyote/concepts/actors/logging) for more information.

## See Also

* namespace [Microsoft.Coyote.Actors](../Microsoft.Coyote.ActorsNamespace.md)
* assembly [Microsoft.Coyote](../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
