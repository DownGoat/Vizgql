using System.Text;
using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder;

public static class TextReport
{
    public static string Create(SchemaType schemaType)
    {
        var sb = new StringBuilder();

        foreach (var rootType in schemaType.RootTypes)
        {
            CreateRootType(sb, rootType);
        }

        return sb.ToString();
    }

    private static void CreateRootType(StringBuilder sb, RootType rootType)
    {
        sb.Append(rootType.Name);
        if (rootType.HasAuthorization)
        {
            sb.Append(' ');
            sb.Append(CreateAuthorizationDirective(rootType.Roles));
        }

        sb.Append('\n');

        for (var i = 0; i < rootType.Fields.Length; i++)
        {
            const string indent = "    ";
            var boxChar = i < rootType.Fields.Length - 1
                ? '├'
                : '└';
            var field = rootType.Fields[i];

            CreateFieldType(sb, indent, boxChar, field);
        }
        
        sb.Append('\n');
    }

    private static void CreateFieldType(StringBuilder sb, string indent, char boxChar, FieldType field)
    {
        sb.Append(indent);
        sb.Append(boxChar);
        sb.Append(' ');
        sb.Append(field.Name);
        sb.Append(' ');
        
        if (field.HasAuthorization)
        {
            sb.Append(CreateAuthorizationDirective(field.Roles));
        }

        sb.Append('\n');
    }

    private static string CreateAuthorizationDirective(IEnumerable<string> roles)
    {
        var rolesText = string.Join(", ", roles.Select(x => $"\"{x}\""));

        return string.IsNullOrEmpty(rolesText)
            ? "@authorize"
            : $"@authorize({rolesText})";
    }
}