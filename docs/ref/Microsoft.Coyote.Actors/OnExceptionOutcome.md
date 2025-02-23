# OnExceptionOutcome enumeration

The outcome when an [`Actor`](./Actor.md) throws an exception.

```csharp
public enum OnExceptionOutcome
```

## Values

| name | value | description |
| --- | --- | --- |
| ThrowException | `0` | The actor throws the exception causing the runtime to fail. |
| HandledException | `1` | The actor handles the exception and resumes execution. |
| Halt | `2` | The actor handles the exception and halts. |

## See Also

* namespace [Microsoft.Coyote.Actors](../Microsoft.Coyote.ActorsNamespace.md)
* assembly [Microsoft.Coyote](../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
