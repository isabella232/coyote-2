# AwaitableEventGroup&lt;T&gt; class

An object representing an awaitable long running context involving one or more actors. An `AwaitableEventGroup` can be provided as an optional argument in CreateActor and SendEvent. If a null `AwaitableEventGroup` is passed then the `EventGroup` is inherited from the sender or target actors (based on which ever one has a [`CurrentEventGroup`](./Actor/CurrentEventGroup.md)). In this way an `AwaitableEventGroup` is automatically communicated to all actors involved in completing some larger operation. Each actor involved can find the `AwaitableEventGroup` using their [`CurrentEventGroup`](./Actor/CurrentEventGroup.md) property.

```csharp
public class AwaitableEventGroup<T> : EventGroup
```

| parameter | description |
| --- | --- |
| T | The result returned when the operation is completed. |

## Public Members

| name | description |
| --- | --- |
| [AwaitableEventGroup](AwaitableEventGroup-1/AwaitableEventGroup.md)(…) | Initializes a new instance of the [`AwaitableEventGroup`](./AwaitableEventGroup-1.md) class. |
| [IsCanceled](AwaitableEventGroup-1/IsCanceled.md) { get; } | Value that indicates whether the task completed execution due to being canceled. |
| [IsCompleted](AwaitableEventGroup-1/IsCompleted.md) { get; } | Indicates the `AwaitableEventGroup` has been completed. |
| [IsFaulted](AwaitableEventGroup-1/IsFaulted.md) { get; } | Value that indicates whether the task completed due to an unhandled exception. |
| [Task](AwaitableEventGroup-1/Task.md) { get; } | Gets the task created by this `AwaitableEventGroup`. |
| [GetAwaiter](AwaitableEventGroup-1/GetAwaiter.md)() | Gets an awaiter for this awaitable. |
| virtual [SetCancelled](AwaitableEventGroup-1/SetCancelled.md)() | Transitions the underlying task into the Canceled state. |
| virtual [SetException](AwaitableEventGroup-1/SetException.md)(…) | Transitions the underlying task into the Faulted state and binds it to the specified exception. |
| virtual [SetResult](AwaitableEventGroup-1/SetResult.md)(…) | Transitions the underlying task into the RanToCompletion state. |
| virtual [TrySetCanceled](AwaitableEventGroup-1/TrySetCanceled.md)() | Attempts to transition the underlying task into the Canceled state. |
| virtual [TrySetException](AwaitableEventGroup-1/TrySetException.md)(…) | Attempts to transition the underlying task into the Faulted state and binds it to the specified exception. |
| virtual [TrySetResult](AwaitableEventGroup-1/TrySetResult.md)(…) | Attempts to transition the underlying task into the RanToCompletion state. |

## See Also

* class [EventGroup](./EventGroup.md)
* namespace [Microsoft.Coyote.Actors](../Microsoft.Coyote.ActorsNamespace.md)
* assembly [Microsoft.Coyote](../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
