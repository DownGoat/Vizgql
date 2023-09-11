namespace SchemaExplorer.Core.Types;

public record SchemaType(RootType[] RootTypes)
{
    public IEnumerable<ValidationAssertion> Validate()
    {
        var validations = new List<ValidationAssertion>();

        foreach (var rootType in RootTypes)
        {
            validations.AddRange(rootType.Validate());
            foreach (var field in rootType.Fields)
            {
                validations.AddRange(field.Validate(rootType));
            }
        }

        return validations;
    }
}
