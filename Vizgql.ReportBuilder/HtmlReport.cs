using System.Diagnostics.CodeAnalysis;
using BlazorTemplater;
using Vizgql.Core.Types;
using Vizgql.ReportBuilder.Html;
using Vizgql.ReportBuilder.Html.Models;

namespace Vizgql.ReportBuilder;

[SuppressMessage("Usage", "Spectre1000:Use AnsiConsole instead of System.Console")]
public static class HtmlReport
{
    public static void Create(SchemaType schemaType)
    {
        var schemaConstraints = new SchemaUniqueConstraints(schemaType);

        var tableModel = GetTableComponentModel(schemaType, schemaConstraints);

        var result = new ComponentRenderer<HtmlReportComponent>()
            .Set(c => c.TableComponentModel, tableModel)
            .Render();

        Console.WriteLine(result);
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

    private static IEnumerable<TableRow> GetTableRows(
        SchemaType schemaType,
        SchemaUniqueConstraints schemaConstraints
    )
    {
        foreach (var rootType in schemaType.RootTypes)
        {
            yield return new TableRow(
                rootType.Name,
                rootType.HasAuthorization,
                GetConstraintsForRow(rootType.Directives, schemaConstraints)
            );

            foreach (var field in rootType.Fields)
            {
                yield return new TableRow(
                    $"{rootType.Name}.{field.Name}",
                    field.HasAuthorization,
                    GetConstraintsForRow(field.Directives, schemaConstraints)
                );
            }
        }
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
