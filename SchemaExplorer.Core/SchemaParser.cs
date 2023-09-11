using GraphQLParser;
using GraphQLParser.AST;
using SchemaExplorer.Core.Types;

namespace SchemaExplorer.Core;

public class SchemaParser
{
    public SchemaType Parse(string schema)
    {
        var document = Parser.Parse(schema);

        var rootTypes = new List<RootType>();

        var schemaDefinition = (GraphQLSchemaDefinition)
            document.Definitions.First(x => x.Kind == ASTNodeKind.SchemaDefinition);

        var rootTypeNames = schemaDefinition.OperationTypes
            .Select(x => x.Type?.Name.Value.ToString())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList<string>();

        foreach (var definition in document.Definitions)
        {
            if (
                definition is GraphQLObjectTypeDefinition typeDefinition
                && rootTypeNames.Contains(typeDefinition.Name.Value.ToString())
            )
            {
                var rootType = ParseRootType(typeDefinition);
                rootTypes.Add(rootType);
            }
        }

        return new SchemaType(rootTypes.ToArray());
    }

    private RootType ParseRootType(GraphQLObjectTypeDefinition typeDefinition)
    {
        bool hasAuthorization = HasAuthorizeDirective(typeDefinition);
        string[] roles = GetRoles(typeDefinition);

        var resolvers = new List<FieldType>();

        foreach (var field in typeDefinition.Fields)
        {
            var fieldType = new FieldType(
                field.Name.Value.ToString(),
                HasAuthorizeDirective(field),
                GetRoles(field)
            );

            resolvers.Add(fieldType);
        }

        resolvers = resolvers.OrderBy(x => x.Name).ToList();

        return new RootType(
            typeDefinition.Name.Value.ToString(),
            hasAuthorization,
            roles,
            resolvers.ToArray()
        );
    }

    private bool HasAuthorizeDirective(GraphQLObjectTypeDefinition typeDefinition)
    {
        return typeDefinition.Directives?.Any(d => d.Name.Value == "authorize") ?? false;
    }

    private bool HasAuthorizeDirective(GraphQLFieldDefinition fieldDefinition)
    {
        return fieldDefinition.Directives?.Any(d => d.Name.Value == "authorize") ?? false;
    }

    private string[] GetRoles(GraphQLObjectTypeDefinition typeDefinition)
    {
        var directive = typeDefinition.Directives?.First(d => d.Name.Value == "authorize");

        if (directive?.Arguments is null)
            return Array.Empty<string>();

        foreach (var arg in directive.Arguments)
        {
            if (arg.Name.Value == "roles")
            {
                var values = ((GraphQLListValue)arg.Value).Values;
                var roles = new List<string>();

                foreach (var value in values)
                {
                    if (value is GraphQLStringValue v)
                    {
                        roles.Add(v.Value.ToString());
                    }
                }

                return roles.ToArray();
            }
        }

        return Array.Empty<string>();
    }

    private string[] GetRoles(GraphQLFieldDefinition fieldDefinition)
    {
        var directive = fieldDefinition.Directives?.First(d => d.Name.Value == "authorize");
        if (directive?.Arguments is null)
            return Array.Empty<string>();
        foreach (var arg in directive.Arguments)
        {
            if (arg.Name.Value == "roles")
            {
                var values = ((GraphQLListValue)arg.Value).Values;
                var roles = new List<string>();

                foreach (var value in values)
                {
                    if (value is GraphQLStringValue v)
                    {
                        roles.Add(v.Value.ToString());
                    }
                }

                return roles.ToArray();
            }
        }

        return Array.Empty<string>();
    }
}
