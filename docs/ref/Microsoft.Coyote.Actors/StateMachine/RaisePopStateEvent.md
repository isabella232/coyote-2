# StateMachine.RaisePopStateEvent method

Raise a special event that performs a pop state operation at the end of the current action.

```csharp
protected void RaisePopStateEvent()
```

## Remarks

Popping a state pops the current [`State`](../StateMachine.State.md) that was pushed using [`RaisePushStateEvent`](./RaisePushStateEvent.md) or an OnEventPushStateAttribute. An assert is raised if there are no states left to pop. This event is not handled until the action that calls this method returns control back to the Coyote runtime. It is handled before any other events are dequeued from the inbox. Only one of the following can be called per action: [`RaiseEvent`](./RaiseEvent.md), [`RaiseGotoStateEvent`](./RaiseGotoStateEvent.md), [`RaisePushStateEvent`](./RaisePushStateEvent.md) or `RaisePopStateEvent` and [`RaiseHaltEvent`](./RaiseHaltEvent.md). An Assert is raised if you accidentally try and do two of these operations in a single action.

## See Also

* class [StateMachine](../StateMachine.md)
* namespace [Microsoft.Coyote.Actors](../StateMachine.md)
* assembly [Microsoft.Coyote](../../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
