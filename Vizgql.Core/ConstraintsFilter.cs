using Vizgql.Core.Types;

namespace Vizgql.Core;

public static class ConstraintsFilter
{
    public static NotAuthorizedTypes GetNotAuthorizedTypes(SchemaType schemaType, Constraints constraints)
    {
        var missingRootTypes = new List<RootType>();
        var missingFieldsByRootType = new Dictionary<RootType, List<FieldType>>();
        
        foreach (var rootType in schemaType.RootTypes)
        {
            if (!constraints.IsAuthorized(rootType.HasAuthorization, rootType.Directives))
            {
                missingRootTypes.Add(rootType);
                continue;
            }

            FindNotAuthorizedFields(constraints, rootType, missingFieldsByRootType);
        }

        return new NotAuthorizedTypes(missingRootTypes, missingFieldsByRootType);
    }

    private static void FindNotAuthorizedFields(Constraints constraints, RootType rootType,
        IDictionary<RootType, List<FieldType>> missingFieldsByRootType)
    {
        foreach (var fieldType in rootType.Fields)
        {
            if (!constraints.IsAuthorized(fieldType.HasAuthorization, fieldType.Directives))
            {
                if (!missingFieldsByRootType.ContainsKey(rootType))
                {
                    missingFieldsByRootType.Add(rootType, []);
                }

                missingFieldsByRootType[rootType].Add(fieldType);
            }
        }
    }
}