namespace Vizgql.Core.Types;

public sealed record ValidationAssertion(string Name, ValidationAssertionType Type);

public enum ValidationAssertionType
{
    // A field or root type is missing authorization directive
    MissingAuthorization,
    
    // A field or root type is missing authorization constraints
    MissingAuthorizationConstraints,
    
    // A field is missing authorization directive
    MissingFieldAuthorization
}

public static class ValidationAssertionTypeDescriptions
{
    public static string ToString(ValidationAssertionType type)
        => type switch
        {
            ValidationAssertionType.MissingAuthorization => "The field or root type is missing authorization",
            ValidationAssertionType.MissingAuthorizationConstraints =>
                "The field or root type is missing authorization constraints",
            ValidationAssertionType.MissingFieldAuthorization => "The field is missing authorization",
        };
}