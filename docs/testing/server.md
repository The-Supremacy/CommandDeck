# Server Testing

The backend target uses xUnit, Shouldly, NSubstitute, Testcontainers, and
architecture tests.

Every backend test should declare a category trait so CI can choose test bands
explicitly:

```csharp
[Trait("Category", "Unit")]
```

Expected categories:

- `Unit`
- `Application`
- `Integration`
- `Eval`

Backend test method names should use `Subject_WhenCondition_ExpectedOutcome`.
Use the behavior or API being exercised as the subject, the relevant state or
event as the condition, and the observable result as the outcome.

Integration tests should use real IO through Testcontainers rather than an
in-memory persistence stack.

The default CI backend job restores, builds, and runs the `Unit` and
`Application` test categories with `--collect:"XPlat Code Coverage"`. Coverage
output is uploaded as a workflow artifact. `Integration` tests remain a local or
explicit validation band because they can require heavier infrastructure such
as Testcontainers.
