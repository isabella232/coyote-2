# Actor.StartTimer method

Starts a timer that sends a [`TimerElapsedEvent`](../../Microsoft.Coyote.Actors.Timers/TimerElapsedEvent.md) to this actor after the specified due time. The timer accepts an optional payload to be used during timeout. The timer is automatically disposed after it timeouts. To manually stop and dispose the timer, invoke the [`StopTimer`](./StopTimer.md) method.

```csharp
protected TimerInfo StartTimer(TimeSpan startDelay, TimerElapsedEvent customEvent = null)
```

| parameter | description |
| --- | --- |
| startDelay | The amount of time to wait before sending the timeout event. |
| customEvent | Optional custom event to raise instead of the default TimerElapsedEvent. |

## Return Value

Handle that contains information about the timer.

## Remarks

See [Using timers in actors](/coyote/concepts/actors/timers) for more information.

## See Also

* class [TimerInfo](../../Microsoft.Coyote.Actors.Timers/TimerInfo.md)
* class [TimerElapsedEvent](../../Microsoft.Coyote.Actors.Timers/TimerElapsedEvent.md)
* class [Actor](../Actor.md)
* namespace [Microsoft.Coyote.Actors](../Actor.md)
* assembly [Microsoft.Coyote](../../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
