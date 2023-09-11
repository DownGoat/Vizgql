using static SchemaExplorer.Core.ValidationAssertionType;

namespace SchemaExplorer.Core;

public record RootType(string Name, bool HasAuthorization, string[] Roles, FieldType[] Fields)
{
    public IEnumerable<ValidationAssertion> Validate()
    {
        if (!HasAuthorization)
        {
            yield return new ValidationAssertion(Name, MissingAuthorization);
        }

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(Name, MissingAuthorizationConstraints);
        }
    }
}

public record FieldType(string Name, bool HasAuthorization, string[] Roles)
{
    public IEnumerable<ValidationAssertion> Validate(RootType parent)
    {
        var name = $"{parent.Name}.{Name}";

        if (!parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(name, MissingAuthorization);
        }

        if (parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(name, MissingFieldAuthorization);
        }

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(name, MissingAuthorizationConstraints);
        }
    }
}

public sealed record ValidationAssertion(string Name, ValidationAssertionType Type);

public enum ValidationAssertionType
{
    MissingAuthorization,
    MissingAuthorizationConstraints,
    MissingFieldAuthorization
}
