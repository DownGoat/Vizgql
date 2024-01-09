using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder;

[SuppressMessage("Usage", "Spectre1000:Use AnsiConsole instead of System.Console")]
public static class CsvReport
{
    public static void Create(SchemaType schemaType)
    {
        var schemaConstraints = new SchemaUniqueConstraints(schemaType);
        var headerNames = string.Join(
            ",",
            schemaConstraints.Roles
                .Select(x => "Role - " + x)
                .Union(schemaConstraints.Policies.Select(x => "Policy - " + x))
        );

        Console.WriteLine("Name,Has Authorization," + headerNames);

        var rootTypes = CreateCsvForRootTypes(schemaType.RootTypes, schemaConstraints);

        foreach (var rootType in rootTypes)
        {
            foreach (var line in rootType)
            {
                Console.WriteLine(line);
            }
        }
    }

    private static IEnumerable<IEnumerable<string>> CreateCsvForRootTypes(
        IEnumerable<RootType> rootTypes,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        return rootTypes.Select(rootType => CreateCsvForRootType(rootType, schemaConstraints));
    }

    private static IEnumerable<string> CreateCsvForRootType(
        RootType rootType,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        var rootTypeConstraints = GetAuthorizationDirectiveConstrains(
            rootType.Directives,
            schemaConstraints
        );
        yield return $"{rootType.Name},{rootType.HasAuthorization},{rootTypeConstraints}";

        foreach (var field in rootType.Fields)
        {
            var fieldConstraints = GetAuthorizationDirectiveConstrains(
                field.Directives,
                schemaConstraints
            );
            yield return $"{rootType.Name}.{field.Name},{field.HasAuthorization},{fieldConstraints}";
        }
    }

    private static string GetAuthorizationDirectiveConstrains(
        AuthorizationDirective[] directives,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        var allRoles = directives.SelectMany(x => x.Roles).Distinct().ToDictionary(k => k);

        var allPolicies = directives.Select(x => x.Policy).Distinct().ToDictionary(k => k);

        var constraints = schemaConstraints.Roles
            .Select(role => allRoles.GetValueOrDefault(role, "False"))
            .Select(value => value.Contains(',') ? $"\"{value}\"" : value)
            .ToList();

        constraints.AddRange(
            schemaConstraints.Policies
                .Select(policy => allPolicies.GetValueOrDefault(policy, "False"))
                .Select(value => value.Contains(',') ? $"\"{value}\"" : value)
        );

        return string.Join(",", constraints);
    }
}
