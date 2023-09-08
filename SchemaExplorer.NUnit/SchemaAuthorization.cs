using SchemaExplorer.ReportBuilder;

namespace SchemaExplorer.NUnit;

public static class SchemaAuthorization
{
    public static void Validate(string sdl)
    {
        var parser = new SchemaParser();

        var results = parser.Parse(sdl);

        foreach (var rootType in results)
        {
            AssertRootTypeAuthorization(rootType, new ValidationOptions());
        }
    }

    public static void AssertRootTypeAuthorization(RootType rootType, ValidationOptions options)
    {
        if (!options.AllowRootTypeWithoutAuthorization && !rootType.HasAuthorization)
        {
            throw new SchemaAuthorizationMissingRootDirective(rootType.Name);
        }

        if (!options.AllowRootTypeEmptyAuthorize && rootType is { HasAuthorization: true, Roles.Length: 0 })
        {
            throw new SchemaAuthorizationMissingRootRoles(rootType.Name);
        }
    }
}

public sealed class SchemaAuthorizationMissingRootDirective : Exception
{
    public SchemaAuthorizationMissingRootDirective(string rootTypeName)
        : base($"The root type '{rootTypeName}' is missing a authorization directive.")
    {
    }
}

public sealed class SchemaAuthorizationMissingRootRoles : Exception
{
    public SchemaAuthorizationMissingRootRoles(string rootTypeName)
        : base($"The root type '{rootTypeName}' is missing roles.")
    {
    }
}

public sealed record ValidationOptions(
    bool AllowRootTypeWithoutAuthorization = false,
    bool AllowRootTypeEmptyAuthorize = true);