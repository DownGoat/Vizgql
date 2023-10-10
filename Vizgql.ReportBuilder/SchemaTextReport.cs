using System.Security.AccessControl;
using System.Text;
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
            CreateConstraintsFilter(schemaType, new Constraints(options.Roles, options.Policies), sb);
        }

        if (options.UniqueConstraints)
        {
            CreateUniqueConstraints(schemaType, sb);
        }

        return sb.ToString();
    }

    private static void CreateUniqueConstraints(SchemaType schemaType, StringBuilder sb)
    {
        var roles = schemaType.RootTypes
            .SelectMany(rt => rt.Fields)
            .SelectMany(ft => ft.Directives)
            .SelectMany(d => d.Roles)
            .Union(schemaType.RootTypes
                .SelectMany(r => r.Directives.SelectMany(rr => rr.Roles)))
            .Distinct();

        var policies = schemaType.RootTypes
            .SelectMany(rt => rt.Fields)
            .SelectMany(ft => ft.Directives)
            .Select(d => d.Policy)
            .Union(schemaType.RootTypes
                .SelectMany(r => r.Directives.Select(rr => rr.Policy)))
            .Distinct();
        
        sb.Append("\nUnique constraints:\n");
        sb.Append("Roles: ");
        sb.Append(string.Join(",", roles));
        sb.Append('\n');
        sb.Append("Policies: ");
        sb.Append(string.Join(",", policies));
    }

    private static void CreateConstraintsFilter(SchemaType schemaType, Constraints constraints, StringBuilder sb)
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
                CreateFieldType(sb, "    ", '├', fieldType);
            }
        }
        
        if (missingRootTypes.Count == 0 && missingFieldsByRootType.Count == 0)
        {
            sb.Append("No missing constraints found.\n");
        }
    }

    private static void CreateRootType(StringBuilder sb, RootType rootType)
    {
        sb.Append(rootType.Name);
        if (rootType.HasAuthorization)
        {
            sb.Append(' ');
            sb.Append(CreateAuthorizationDirectives(rootType.Directives));
        }

        sb.Append('\n');

        for (var i = 0; i < rootType.Fields.Length; i++)
        {
            const string indent = "    ";
            var boxChar = i < rootType.Fields.Length - 1 ? '├' : '└';
            var field = rootType.Fields[i];

            CreateFieldType(sb, indent, boxChar, field);
        }

        sb.Append('\n');
    }

    private static void CreateFieldType(
        StringBuilder sb,
        string indent,
        char boxChar,
        FieldType field
    )
    {
        sb.Append(indent);
        sb.Append(boxChar);
        sb.Append(' ');
        sb.Append(field.Name);
        sb.Append(' ');

        if (field.HasAuthorization)
        {
            sb.Append(CreateAuthorizationDirectives(field.Directives));
        }

        sb.Append('\n');
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
            var roles = string.Join(", ", directive.Roles.Select(x => $"\"{x}\""));
            content = $"(roles: [ {roles} ])";
        }

        if (!string.IsNullOrEmpty(directive.Policy))
        {
            content = $"(policy: \"{directive.Policy}\")";
        }

        return "@authorize" + content;
    }

    private static string CreateValidations(SchemaType schemaType, StringBuilder sb)
    {
        var validations = schemaType.Validate();

        sb.Append("\nValidations errors:\n");
        foreach (var validationAssertion in validations)
        {
            sb.Append(
                $"{validationAssertion.Name} - {ValidationAssertionTypeDescriptions.ToString(validationAssertion.Type)}\n");
        }

        return sb.ToString();
    }
}

public readonly record struct SchemaTextReportOptions(bool Validations, string?[] Roles, string?[] Policies,
    bool UniqueConstraints);

public record Constraints(string[]? Roles, string[]? Policies)
{
    public bool IsAuthorized(bool hasAuthorization, AuthorizationDirective[] authorizationDirectives)
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