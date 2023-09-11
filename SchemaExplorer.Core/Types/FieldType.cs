namespace SchemaExplorer.Core.Types;

public record FieldType(string Name, bool HasAuthorization, string[] Roles)
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

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(
                name,
                ValidationAssertionType.MissingAuthorizationConstraints
            );
        }
    }
}
