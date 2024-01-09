using System.Text;
using Spectre.Console;
using Vizgql.Core;
using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder;

public static class SchemaTextReport
{
    public static void Create(SchemaType schemaType, SchemaTextReportOptions options)
    {
        var overviewTree = new Tree("Schema");
        foreach (var rootType in schemaType.RootTypes)
        {
            CreateRootType(overviewTree, rootType);
        }

        var rule = new Rule();
        AnsiConsole.Write(overviewTree);
        AnsiConsole.Write(rule);

        if (options.Validations)
        {
            CreateValidations(schemaType);
            AnsiConsole.Write(rule);
        }

        if (options.Roles != null || options.Policies != null)
        {
            CreateConstraintsFilter(schemaType, new Constraints(options.Roles, options.Policies));
            AnsiConsole.Write(rule);
        }

        if (options.UniqueConstraints)
        {
            CreateUniqueConstraints(schemaType);
            AnsiConsole.Write(rule);
        }
    }

    private static void CreateUniqueConstraints(SchemaType schemaType)
    {
        var schemaUniqueConstraints = new SchemaUniqueConstraints(schemaType);

        var roles = string.Join(",", schemaUniqueConstraints.Roles.Select(x => $"[green]{x}[/]"));
        var rolesPanel = new Panel(roles)
        {
            Expand = true,
            Header = new PanelHeader("Unique Roles")
        };

        AnsiConsole.Write(rolesPanel);

        var policies = string.Join(
            ",",
            schemaUniqueConstraints.Policies.Select(x => $"[green]{x}[/]")
        );
        var policiesPanel = new Panel(policies)
        {
            Expand = true,
            Header = new PanelHeader("Unique Policies")
        };

        AnsiConsole.Write(policiesPanel);
    }

    private static void CreateConstraintsFilter(SchemaType schemaType, Constraints constraints)
    {
        var tree = new Tree("Constraints filter");

        var notAuthorizedTypes = ConstraintsFilter.GetNotAuthorizedTypes(schemaType, constraints);
        if (notAuthorizedTypes.NotAuthorizedRootTypes.Any())
        {
            var rows = new Rows(notAuthorizedTypes.NotAuthorizedRootTypes.Select(m => new Markup($"[red]{m.Name}[/]")));
            var missingRootTypesPanel = new Panel(rows)
            {
                Header = new PanelHeader("Unauthorized root types"),
                BorderStyle = new Style(Color.HotPink),
                Expand = true
            };
            AnsiConsole.Write(missingRootTypesPanel);
        }
        
        foreach (var (rootType, fieldTypes) in notAuthorizedTypes.NotAuthorizedFieldsByRootType)
        {
            var withMissing = rootType with { Fields = fieldTypes.ToArray() };
            CreateRootType(tree, withMissing);
        }
        
        if (notAuthorizedTypes.NotAuthorizedRootTypes.Count == 0 && notAuthorizedTypes.NotAuthorizedFieldsByRootType.Count == 0)
        {
            var panel = new Panel(new Markup($"[green]No missing constraints found.[/]"))
            {
                BorderStyle = new Style(Color.Green)
            };
            AnsiConsole.Write(panel);
        }
        
        if (notAuthorizedTypes.NotAuthorizedFieldsByRootType.Any())
            AnsiConsole.Write(tree);
    }

    private static void CreateRootType(Tree tree, RootType rootType)
    {
        var rootName = rootType.Name;
        if (rootType.HasAuthorization)
        {
            var directives = CreateAuthorizationDirectives(rootType.Directives);
            rootName = $"[magenta1]{rootName}[/] {directives}";
        }
        var root = new Tree(rootName);

        foreach (var field in rootType.Fields)
        {
            root.AddNode(CreateFieldType(field));
        }

        tree.AddNode(root);
    }

    private static string CreateFieldType(FieldType field)
    {
        var fieldName = $"[magenta1]{field.Name}[/]";
        if (field.HasAuthorization)
        {
            var directives = CreateAuthorizationDirectives(field.Directives);
            fieldName = $"{fieldName} {directives}";
        }

        return fieldName;
    }

    private static string CreateAuthorizationDirectives(AuthorizationDirective[] roles)
    {
        return string.Join(" ", roles.Select(CreateAuthorizationDirective));
    }

    private static string CreateAuthorizationDirective(AuthorizationDirective directive)
    {
        var content = string.Empty;

        if (directive.Roles.Length != 0)
        {
            var roles = string.Join(", ", directive.Roles.Select(x => $"[green]\"{x}\"[/]"));
            content = $"([mediumpurple1]roles[/]: [[ {roles} ]])";
        }

        if (!string.IsNullOrEmpty(directive.Policy))
        {
            content = $"([mediumpurple1]policy[/]: [green]\"{directive.Policy}\"[/])";
        }

        return "[yellow3_1]@authorize" + content + "[/]";
    }

    private static void CreateValidations(SchemaType schemaType)
    {
        var validations = schemaType.Validate();

        if (!validations.Any())
            return;

        var rows = new Rows(
            validations.Select(
                v =>
                    new Markup(
                        $"{v.Name} - [hotpink]{ValidationAssertionTypeDescriptions.ToString(v.Type)}[/]"
                    )
            )
        );

        var panel = new Panel(rows)
        {
            Header = new PanelHeader("[red]Validation errors[/]"),
            Expand = true,
            BorderStyle = new Style(Color.Red)
        };

        AnsiConsole.Write(panel);
    }
}

public readonly record struct SchemaTextReportOptions(
    bool Validations,
    string?[] Roles,
    string?[] Policies,
    bool UniqueConstraints
);


