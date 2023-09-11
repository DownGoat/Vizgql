namespace SchemaExplorer.Core.Types;

public record RootType(string Name, bool HasAuthorization, string[] Roles, FieldType[] Fields)
{
    public IEnumerable<ValidationAssertion> Validate()
    {
        if (!HasAuthorization)
        {
            yield return new ValidationAssertion(
                Name,
                ValidationAssertionType.MissingAuthorization
            );
        }

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(
                Name,
                ValidationAssertionType.MissingAuthorizationConstraints
            );
        }
    }
}
