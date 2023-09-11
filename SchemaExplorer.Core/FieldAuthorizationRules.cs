using static SchemaExplorer.Core.ValidationAssertionType;

namespace SchemaExplorer.Core;

public static class FieldAuthorizationRules
{
    public static bool MissingAuthorizationDirective(IEnumerable<ValidationAssertion> validations)
    {
        return validations.Any(x => x.Type == MissingAuthorization);
    }

    public static bool FieldMissingAuthorization(
        IEnumerable<ValidationAssertion> validations,
        ValidationOptions options
    )
    {
        return !options.AllowFieldWithoutAuthorization
            && validations.Any(x => x.Type == MissingFieldAuthorization);
    }

    public static bool MissingConstraints(IEnumerable<ValidationAssertion> validations)
    {
        return validations.Any(x => x.Type == MissingAuthorizationConstraints);
    }
}
