namespace Vizgql.Core.Types;

public sealed record ValidationOptions(
    bool AllowRootTypeWithoutAuthorization = false,
    bool AllowRootTypeEmptyAuthorize = true,
    bool AllowFieldWithoutAuthorization = false
);
