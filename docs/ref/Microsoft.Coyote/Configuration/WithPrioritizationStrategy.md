# Configuration.WithPrioritizationStrategy method

Updates the configuration to use the priority-based scheduling strategy during systematic testing. You can specify if you want to enable liveness checking, which is disabled by default, and an upper bound of possible priority changes, which by default can be up to 10.

```csharp
public Configuration WithPrioritizationStrategy(bool isFair = false, uint priorityChangeBound = 10)
```

| parameter | description |
| --- | --- |
| isFair | If true, enable liveness checking by using fair scheduling. |
| priorityChangeBound | Upper bound of possible priority changes per test iteration. |

## See Also

* class [Configuration](../Configuration.md)
* namespace [Microsoft.Coyote](../Configuration.md)
* assembly [Microsoft.Coyote](../../Microsoft.Coyote.md)

<!-- DO NOT EDIT: generated by xmldocmd for Microsoft.Coyote.dll -->
