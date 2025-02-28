# Configuration.WithDeadlockTimeout method

Updates the value that controls how much time the deadlock monitor should wait during concurrency testing before reporting a potential deadlock.

```csharp
public Configuration WithDeadlockTimeout(uint timeout)
```

| parameter | description |
| --- | --- |
| timeout | The timeout value in milliseconds, which by default is 5000. |

## Remarks

Increase the value to give more time to the test to resolve a potential deadlock.

## See Also

* class [Configuration](../Configuration.md)
* namespace [Microsoft.Coyote](../Configuration.md)
* assembly [Microsoft.Coyote](../../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
