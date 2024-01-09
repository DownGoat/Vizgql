namespace Vizgql.Core.Types;

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
