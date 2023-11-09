using static Vizgql.Core.Types.ValidationAssertionType;

namespace Vizgql.Core.Types;

public record SchemaType(RootType[] RootTypes)
{
    public IEnumerable<ValidationAssertion> Validate()
    {
        var validations = new List<ValidationAssertion>();

        FieldValidations(validations);
        ConstraintsValidations(validations);

        return validations;
    }

    private void ConstraintsValidations(List<ValidationAssertion> validations)
    {
        var schemaUniqueConstraints = new SchemaUniqueConstraints(this);

        var union = schemaUniqueConstraints.RolesDistances.Union(
            schemaUniqueConstraints.PoliciesDistances
        );

        foreach (var rolesDistance in union)
        {
            validations.Add(
                new ValidationAssertion(
                    $"{rolesDistance.c1} - {rolesDistance.c2}",
                    ConstraintSpellingMistake
                )
            );
        }
    }

    private void FieldValidations(List<ValidationAssertion> validations)
    {
        foreach (var rootType in RootTypes)
        {
            validations.AddRange(rootType.Validate());
            foreach (var field in rootType.Fields)
            {
                validations.AddRange(field.Validate(rootType));
            }
        }
    }
}
