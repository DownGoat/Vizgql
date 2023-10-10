using System.Text;
using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder;

public static class SchemaTextReport
{
    public static string Create(SchemaType schemaType, bool validations)
    {
        var sb = new StringBuilder();

        foreach (var rootType in schemaType.RootTypes)
        {
            CreateRootType(sb, rootType);
        }
        
        if (validations)
        {
            sb.Append(ValidationsTextReport.Create(schemaType));
        }

        return sb.ToString();
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
}
