using SchemaExplorer.ReportBuilder;
using static SchemaExplorer.ReportBuilder.ValidationAssertionType;

namespace SchemaExplorer.NUnit;

public static class SchemaAuthorization
{
    public static void Validate(string sdl, ValidationOptions validationOptions)
    {
        var parser = new SchemaParser();

        var results = parser.Parse(sdl);

        foreach (var rootType in results)
        {
            AssertRootTypeAuthorization(rootType, validationOptions);
        }
    }

    private static void AssertRootTypeAuthorization(RootType rootType, ValidationOptions options)
    {
        if (!options.AllowRootTypeWithoutAuthorization && !rootType.HasAuthorization)
        {
            throw new SchemaAuthorizationMissingAuthorization(rootType.Name);
        }

        if (!options.AllowRootTypeEmptyAuthorize && rootType is { HasAuthorization: true, Roles.Length: 0 })
        {
            throw new SchemaAuthorizationMissingConstraints(rootType.Name);
        }

        foreach (var field in rootType.Resolvers)
        {
            AssertFieldAuthorization(field, rootType);
        }
    }

    private static void AssertFieldAuthorization(FieldType field, RootType rootType)
    {
        var validations = field.Validate(rootType).ToArray();

        if (validations.Any(x => x.Type == MissingAuthorization))
        {
            throw new SchemaAuthorizationMissingAuthorization($"{rootType.Name} => {field.Name}");
        }

        if (validations.Any(x => x.Type == MissingAuthorizationConstraints))
        {
          throw new SchemaAuthorizationMissingConstraints($"{rootType.Name} => {field.Name}"); 
        }

        if (validations.Any(x => x.Type == MissingFieldAuthorization))
        {
            throw new SchemaAuthorizationMissingFieldAuthorization($"{rootType.Name} => {field.Name}"); 
        }
    }
}

public sealed class SchemaAuthorizationMissingAuthorization : Exception
{
    public SchemaAuthorizationMissingAuthorization(string name)
        : base($"The type/field '{name}' is missing a authorization directive.")
    {
    }
}

public sealed class SchemaAuthorizationMissingConstraints : Exception
{
    public SchemaAuthorizationMissingConstraints(string name)
        : base($"The type/field '{name}' is missing constraints like roles/policies.")
    {
    }
}

public sealed class SchemaAuthorizationMissingFieldAuthorization : Exception
{
    public SchemaAuthorizationMissingFieldAuthorization(string fieldName)
        : base($"The field '{fieldName}' is missing authorization.")
    {
    }
}

public sealed record ValidationOptions(
    bool AllowRootTypeWithoutAuthorization = false,
    bool AllowRootTypeEmptyAuthorize = true);