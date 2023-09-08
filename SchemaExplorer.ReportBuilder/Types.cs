namespace SchemaExplorer.ReportBuilder;

public record RootType(string Name, bool HasAuthorization, string[] Roles, FieldType[] Resolvers)
{
    public IEnumerable<ValidationAssertion> Validate()
    {
        if (!HasAuthorization)
        {
            yield return new ValidationAssertion(Name, ValidationAssertionType.MissingAuthorization);
        }

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(Name, ValidationAssertionType.MissingAuthorizationConstraints);
        }

        // foreach (var field in Resolvers)
        // {
        //     foreach (var fieldAssertion in field.Validate(this))
        //     {
        //         yield return fieldAssertion;
        //     }
        // }
    }
}
public record FieldType(string Name, bool HasAuthorization, string[] Roles)
{
    public IEnumerable<ValidationAssertion> Validate(RootType parent)
    {
        var name = $"{parent.Name}.Name";
        
        if (!parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(name, ValidationAssertionType.MissingAuthorization);
        }

        if (parent.HasAuthorization && !HasAuthorization)
        {
            yield return new ValidationAssertion(name, ValidationAssertionType.MissingFieldAuthorization);
        }

        if (HasAuthorization && Roles.Length == 0)
        {
            yield return new ValidationAssertion(name, ValidationAssertionType.MissingAuthorizationConstraints);
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