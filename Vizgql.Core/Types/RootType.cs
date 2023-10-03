namespace Vizgql.Core.Types;

public record RootType(
    string Name,
    bool HasAuthorization,
    AuthorizationDirective[] Directives,
    FieldType[] Fields
)
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

        if (HasAuthorization && IsMissingConstraints())
        {
            yield return new ValidationAssertion(
                Name,
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
