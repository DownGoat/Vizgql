using System.Diagnostics.CodeAnalysis;
using BlazorTemplater;
using Vizgql.Core.Types;
using Vizgql.ReportBuilder.Html;
using Vizgql.ReportBuilder.Html.Models;

namespace Vizgql.ReportBuilder;

[SuppressMessage("Usage", "Spectre1000:Use AnsiConsole instead of System.Console")]
public static class HtmlReport
{
    public static void Create(SchemaType schemaType, HtmlReportComponentOptions options)
    {
        var schemaConstraints = new SchemaUniqueConstraints(schemaType);

        var tableModel = GetTableComponentModel(schemaType, schemaConstraints);
        var uniqueConstraintsModel = GetUniqueConstraintsModel(schemaConstraints);
        var validationsModel = GetValidationsModel(schemaType);
        
        var result = new ComponentRenderer<HtmlReportComponent>()
            .Set(c => c.Model, new HtmlReportComponentModel(
                tableModel,
                uniqueConstraintsModel,
                validationsModel,
                options
                ))
            .Render();

        Console.WriteLine(result);
    }

    private static ValidationsComponentModel? GetValidationsModel(SchemaType schemaType)
    {
        var validations = schemaType.Validate();
        return new ValidationsComponentModel(validations.ToArray());
    }

    private static UniqueConstraintsComponentModel? GetUniqueConstraintsModel(SchemaUniqueConstraints schemaConstraints)
    {
        return new UniqueConstraintsComponentModel(schemaConstraints.Roles, schemaConstraints.Policies);
    }

    private static TableComponentModel GetTableComponentModel(
        SchemaType schemaType,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        var headerConstraints = GetHeaderConstraints(schemaConstraints);
        var tableRows = GetTableRows(schemaType, schemaConstraints);

        return new TableComponentModel(headerConstraints, tableRows.ToArray());
    }

    private static IEnumerable<RootTypeGroup> GetTableRows(
        SchemaType schemaType,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        foreach (var rootType in schemaType.RootTypes)
        {
            yield return new RootTypeGroup(
                rootType.Name,
                rootType.HasAuthorization,
                GetConstraintsForRow(rootType.Directives, schemaConstraints),
                GetRootTypeRows(rootType.Fields, schemaConstraints).ToArray()
                );
        }
    }

    private static IEnumerable<TableRow> GetRootTypeRows(
        IEnumerable<FieldType> fieldTypes,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        return fieldTypes.Select(fieldType => new TableRow(
            fieldType.Name,
            fieldType.HasAuthorization,
            GetConstraintsForRow(fieldType.Directives, schemaConstraints)
        ));
    }

    private static string[] GetConstraintsForRow(
        AuthorizationDirective[] directives,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        var allRoles = directives.SelectMany(x => x.Roles).Distinct().ToDictionary(k => k);

        var allPolicies = directives.Select(x => x.Policy).Distinct().ToDictionary(k => k);

        var constraints = schemaConstraints.Roles
            .Select(role => allRoles.GetValueOrDefault(role, string.Empty))
            .Select(value => value.Contains(',') ? $"\"{value}\"" : value)
            .ToList();

        constraints.AddRange(
            schemaConstraints.Policies.Select(
                policy => allPolicies.GetValueOrDefault(policy, string.Empty)
            )
        );

        return constraints.ToArray();
    }

    private static ConstraintHeader[] GetHeaderConstraints(
        SchemaUniqueConstraints schemaConstraints
    )
    {
        return schemaConstraints.Roles
            .Select(x => new ConstraintHeader(x, "role"))
            .Union(schemaConstraints.Policies.Select(x => new ConstraintHeader(x, "policy")))
            .ToArray();
    }
}
