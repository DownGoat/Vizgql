namespace Vizgql.Core.Types;

public sealed class SchemaUniqueConstraints
{
    private readonly SchemaType _schemaType;
    public string[] Roles { get; private set; }
    public string[] Policies { get; private set; }
    public (string c1, string c2, int distance)[] RolesDistances { get; private set; }
    public (string c1, string c2, int distance)[] PoliciesDistances { get; private set; }

    public SchemaUniqueConstraints(SchemaType schemaType)
    {
        _schemaType = schemaType;
        Roles = GetRoles();
        Policies = GetPolicies();
        RolesDistances = CalculateLevenshteinDistances(Roles);
        PoliciesDistances = CalculateLevenshteinDistances(Policies);
    }

    private string[] GetRoles()
    {
        return _schemaType.RootTypes
            .SelectMany(rt => rt.Fields)
            .SelectMany(ft => ft.Directives)
            .SelectMany(d => d.Roles)
            .Union(_schemaType.RootTypes.SelectMany(r => r.Directives.SelectMany(rr => rr.Roles)))
            .Order()
            .Distinct()
            .ToArray();
    }

    private string[] GetPolicies()
    {
        return _schemaType.RootTypes
            .SelectMany(rt => rt.Fields)
            .SelectMany(ft => ft.Directives)
            .Select(d => d.Policy)
            .Union(_schemaType.RootTypes.SelectMany(r => r.Directives.Select(rr => rr.Policy)))
            .Order()
            .Distinct()
            .ToArray();
    }

    private (string c1, string c2, int distance)[] CalculateLevenshteinDistances(
        string[] constraints
    )
    {
        var results = new List<(string c1, string c2, int distance)>();
        foreach (var constraint in constraints)
        {
            foreach (var comp in constraints)
            {
                if (constraint == comp)
                    continue;

                var distance = CalculateLevenshteinDistance(constraint, comp);
                results.Add(distance);
            }
        }

        results = results.Where(x => x.distance <= 2).ToList();

        return results.ToArray();
    }

    private static (string c1, string c2, int distance) CalculateLevenshteinDistance(
        string constraint,
        string comp
    )
    {
        // Initialize the matrix
        var matrix = new int[constraint.Length + 1, comp.Length + 1];
        for (var i = 1; i <= constraint.Length; i++)
            matrix[i, 0] = i;
        for (var j = 1; j <= comp.Length; j++)
            matrix[0, j] = j;

        // Calculate the Levenshtein distance
        for (var i = 1; i <= constraint.Length; i++)
        {
            for (var j = 1; j <= comp.Length; j++)
            {
                var cost = (constraint[i - 1] == comp[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        // The Levenshtein distance is the value in the bottom-right cell of the matrix
        var distance = matrix[constraint.Length, comp.Length];

        // Return the result as a tuple
        return (constraint, comp, distance);
    }
}
