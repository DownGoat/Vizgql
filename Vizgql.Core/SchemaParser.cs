using GraphQLParser;
using GraphQLParser.AST;
using Vizgql.Core.Types;

namespace Vizgql.Core;

public class SchemaParser
{
    private const string AuthorizeDirectiveName = "authorize";
    private const string RolesArgumentName = "roles";
    private const string PolicyArgumentName = "policy";

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

    private static AuthorizationDirective[] ExtractDirective(GraphQLDirective directive)
    {
        if (directive.Arguments is null)
            return new[] { new AuthorizationDirective(Array.Empty<string>(), string.Empty) };

        var rolesArguments = directive.Arguments.Where(x => x.Name.Value == RolesArgumentName);
        var policyArgument = directive.Arguments.Where(x => x.Name.Value == PolicyArgumentName);

        var roles = rolesArguments
            .Select(arg => ((GraphQLListValue)arg.Value).Values ?? new List<GraphQLValue>())
            .Select(values => values.Where(v => v is GraphQLStringValue))
            .Select(values => values.Select(v => ((GraphQLStringValue)v).Value.ToString()));

        var polices = policyArgument
            .Select(arg => arg.Value)
            .Select(v => ((GraphQLStringValue)v).Value.ToString());

        var roleDirectives = roles.Select(
            r => new AuthorizationDirective(r.ToArray(), string.Empty)
        );

        var policyDirectives = polices.Select(
            p => new AuthorizationDirective(Array.Empty<string>(), p)
        );

        return roleDirectives.Union(policyDirectives).ToArray();
    }

    private static AuthorizationDirective[] GetRoles(GraphQLObjectTypeDefinition typeDefinition)
    {
        var directives =
            typeDefinition.Directives?.Where(d => d.Name.Value == AuthorizeDirectiveName)
            ?? new List<GraphQLDirective>();

        return directives.SelectMany(ExtractDirective).ToArray();
    }

    private static AuthorizationDirective[] GetRoles(IHasDirectivesNode fieldDefinition)
    {
        var directives =
            fieldDefinition.Directives?.Where(d => d.Name.Value == AuthorizeDirectiveName)
            ?? new List<GraphQLDirective>();

        return directives.SelectMany(ExtractDirective).ToArray();
    }
}
