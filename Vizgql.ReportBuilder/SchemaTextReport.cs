using System.Text;
using Spectre.Console;
using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder;

public static class SchemaTextReport
{
    public static string Create(SchemaType schemaType, SchemaTextReportOptions options)
    {
        var sb = new StringBuilder();

        foreach (var rootType in schemaType.RootTypes)
        {
            CreateRootType(sb, rootType);
        }

        if (options.Validations)
        {
            CreateValidations(schemaType, sb);
        }

        if (options.Roles != null || options.Policies != null)
        {
            CreateConstraintsFilter(
                schemaType,
                new Constraints(options.Roles, options.Policies),
                sb
            );
        }

        if (options.UniqueConstraints)
        {
            CreateUniqueConstraints(schemaType, sb);
        }

        return sb.ToString();
    }

    private static void CreateUniqueConstraints(SchemaType schemaType, StringBuilder sb)
    {
        var schemaUniqueConstraints = new SchemaUniqueConstraints(schemaType);

        sb.Append("\nUnique constraints:\n");
        sb.Append("Roles: ");
        sb.Append(string.Join(",", schemaUniqueConstraints.Roles));

        if (schemaUniqueConstraints.RolesDistances.Any())
        {
            sb.Append('\n');
            sb.Append("These roles are similar and might be spelling mistakes: ");
            sb.Append(string.Join(", ", schemaUniqueConstraints.RolesDistances.Select(x => x.c1)));
        }

        sb.Append('\n');
        sb.Append("Policies: ");
        sb.Append(string.Join(",", schemaUniqueConstraints.Policies));

        if (schemaUniqueConstraints.PoliciesDistances.Any())
        {
            sb.Append('\n');
            sb.Append("These policies are similar and might be spelling mistakes: ");
            sb.Append(
                string.Join(", ", schemaUniqueConstraints.PoliciesDistances.Select(x => x.c1))
            );
        }
    }

    private static void CreateConstraintsFilter(
        SchemaType schemaType,
        Constraints constraints,
        StringBuilder sb
    )
    {
        sb.Append("\nConstraints filter:\n");

        var missingRootTypes = new List<RootType>();
        var missingFieldsByRootType = new Dictionary<RootType, List<FieldType>>();
        foreach (var rootType in schemaType.RootTypes)
        {
            if (!constraints.IsAuthorized(rootType.HasAuthorization, rootType.Directives))
            {
                missingRootTypes.Add(rootType);
                continue;
            }

            foreach (var fieldType in rootType.Fields)
            {
                if (!constraints.IsAuthorized(fieldType.HasAuthorization, fieldType.Directives))
                {
                    if (!missingFieldsByRootType.ContainsKey(rootType))
                    {
                        missingFieldsByRootType.Add(rootType, new List<FieldType>());
                    }

                    missingFieldsByRootType[rootType].Add(fieldType);
                }
            }
        }

        foreach (var missingRootType in missingRootTypes)
        {
            sb.Append($"Missing authorization for root type {missingRootType.Name}\n");
        }

        foreach (var (rootType, fieldTypes) in missingFieldsByRootType)
        {
            sb.Append($"\n{rootType.Name}:\n");
            foreach (var fieldType in fieldTypes)
            {
                // CreateFieldType(sb, "    ", '├', fieldType);
            }
        }

        if (missingRootTypes.Count == 0 && missingFieldsByRootType.Count == 0)
        {
            sb.Append("No missing constraints found.\n");
        }
    }

    private static void CreateRootType(StringBuilder sb, RootType rootType)
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

        var rule = new Rule();
        AnsiConsole.Write(root);
        AnsiConsole.Write(rule);
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

    private static void CreateValidations(SchemaType schemaType, StringBuilder sb)
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

public record Constraints(string[]? Roles, string[]? Policies)
{
    public bool IsAuthorized(
        bool hasAuthorization,
        AuthorizationDirective[] authorizationDirectives
    )
    {
        if (!hasAuthorization || authorizationDirectives.All(ad => ad.IsEmpty()))
        {
            return true;
        }

        foreach (var authorizationDirective in authorizationDirectives)
        {
            if (Roles != null && Roles.Length != 0)
            {
                if (authorizationDirective.Roles.Any(r => Roles.Contains(r)))
                {
                    return true;
                }
            }

            if (Policies != null && Policies.Length != 0)
            {
                if (Policies.Contains(authorizationDirective.Policy))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
