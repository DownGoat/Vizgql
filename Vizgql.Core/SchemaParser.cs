using GraphQLParser;
using GraphQLParser.AST;
using Vizgql.Core.Types;

namespace Vizgql.Core;

public class SchemaParser
{
    private const string AuthorizeDirectiveName = "authorize";
    private const string RolesArgumentName = "roles";

    public static SchemaType Parse(string schema)
    {
        var document = Parser.Parse(schema);

        var rootTypes = new List<RootType>();

        var schemaDefinition = (GraphQLSchemaDefinition)
            document.Definitions.First(x => x.Kind == ASTNodeKind.SchemaDefinition);

        var rootTypeNames = schemaDefinition.OperationTypes
            .Select(x => x.Type?.Name.Value.ToString())
            .Where(x => !string.IsNullOrEmpty(x))!
            .ToList<string>();

        foreach (var definition in document.Definitions)
        {
            if (
                definition is GraphQLObjectTypeDefinition typeDefinition
                && IsDefinitionRootType(rootTypeNames, typeDefinition)
            )
            {
                var rootType = ParseRootType(typeDefinition);
                rootTypes.Add(rootType);
            }
        }

        return new SchemaType(rootTypes.ToArray());
    }

    private static bool IsDefinitionRootType(
        ICollection<string> rootTypeNames,
        INamedNode typeDefinition
    )
    {
        return rootTypeNames.Contains(typeDefinition.Name.Value.ToString());
    }

    private static RootType ParseRootType(GraphQLObjectTypeDefinition typeDefinition)
    {
        var hasAuthorization = HasAuthorizeDirective(typeDefinition);
        var roles = GetRoles(typeDefinition);

        var resolvers = new List<FieldType>();

        if (typeDefinition.Fields is null)
            return new RootType(
                typeDefinition.Name.Value.ToString(),
                hasAuthorization,
                roles,
                resolvers.ToArray()
            );

        resolvers.AddRange(
            typeDefinition.Fields.Select(
                field =>
                    new FieldType(
                        field.Name.Value.ToString(),
                        HasAuthorizeDirective(field),
                        GetRoles(field)
                    )
            )
        );

        resolvers = resolvers.OrderBy(x => x.Name).ToList();

        return new RootType(
            typeDefinition.Name.Value.ToString(),
            hasAuthorization,
            roles,
            resolvers.ToArray()
        );
    }

    private static bool HasAuthorizeDirective(GraphQLObjectTypeDefinition typeDefinition)
    {
        return typeDefinition.Directives?.Any(d => d.Name.Value == AuthorizeDirectiveName) ?? false;
    }

    private static bool HasAuthorizeDirective(IHasDirectivesNode fieldDefinition)
    {
        return fieldDefinition.Directives?.Any(d => d.Name.Value == AuthorizeDirectiveName)
            ?? false;
    }

    private static string[] GetRoles(GraphQLObjectTypeDefinition typeDefinition)
    {
        var directive = typeDefinition.Directives?.First(
            d => d.Name.Value == AuthorizeDirectiveName
        );

        if (directive?.Arguments is null)
            return Array.Empty<string>();

        foreach (var arg in directive.Arguments)
        {
            if (arg.Name.Value != RolesArgumentName)
                continue;

            var values = ((GraphQLListValue)arg.Value).Values;
            if (values is null || values.Count == 0)
                continue;

            return GetGraphQlValues(values).ToArray();
        }

        return Array.Empty<string>();
    }

    private static IEnumerable<string> GetGraphQlValues(List<GraphQLValue> values)
    {
        foreach (var value in values)
        {
            if (value is GraphQLStringValue v)
            {
                yield return v.Value.ToString();
            }
        }
    }

    private static string[] GetRoles(IHasDirectivesNode fieldDefinition)
    {
        var directive = fieldDefinition.Directives?.First(
            d => d.Name.Value == AuthorizeDirectiveName
        );

        if (directive?.Arguments is null)
            return Array.Empty<string>();

        foreach (var arg in directive.Arguments)
        {
            if (arg.Name.Value != RolesArgumentName)
                continue;

            var values = ((GraphQLListValue)arg.Value).Values;
            if (values is null || values.Count == 0)
                continue;

            return GetGraphQlValues(values).ToArray();
        }

        return Array.Empty<string>();
    }
}
