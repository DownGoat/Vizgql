namespace SchemaExplorer.Core.Types;

public sealed record ValidationAssertion(string Name, ValidationAssertionType Type);

public enum ValidationAssertionType
{
    MissingAuthorization,
    MissingAuthorizationConstraints,
    MissingFieldAuthorization
}
