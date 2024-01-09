namespace Vizgql.Core.Types;

public sealed record NotAuthorizedTypes(List<RootType> NotAuthorizedRootTypes, Dictionary<RootType, List<FieldType>> NotAuthorizedFieldsByRootType);