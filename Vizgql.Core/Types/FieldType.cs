namespace Vizgql.Core.Types;

public record FieldType(string Name, bool HasAuthorization, AuthorizationDirective[] Directives)
{
    public IEnumerable<ValidationAssertion> Validate(RootType parent)
    {
        var name = $"{parent.Name}.{Name}";

        if (!parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(
                name,
                ValidationAssertionType.MissingAuthorization
            );
        }

        if (parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(
                name,
                ValidationAssertionType.MissingFieldAuthorization
            );
        }

        if (HasAuthorization && IsMissingConstraints())
        {
            yield return new ValidationAssertion(
                name,
                ValidationAssertionType.MissingAuthorizationConstraints
            );
        }
    }

    public bool IsMissingConstraints()
    {
        return Directives.Length == 0
            || Directives.All(d => d.Roles.Length == 0 && string.IsNullOrEmpty(d.Policy));
    }
}

public record AuthorizationDirective(string[] Roles, string Policy);
