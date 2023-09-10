using SchemaExplorer.Core.Exceptions;
using SchemaExplorer.NUnit;
using static SchemaExplorer.Core.ValidationAssertionType;

namespace SchemaExplorer.Core;

public static class SchemaAuthorization
{
    public static void AssertValidate(string sdl, ValidationOptions validationOptions)
    {
        var parser = new SchemaParser();

        var results = parser.Parse(sdl);

        foreach (var rootType in results)
        {
            AssertRootTypeAuthorization(rootType, validationOptions);
        }
    }
    
    public static IEnumerable<ValidationAssertion> Validate(string sdl)
    {
        var parser = new SchemaParser();

        var results = parser.Parse(sdl);

        foreach (var rootType in results)
        {
            foreach (var validationAssertion in rootType.Validate())
            {
                yield return validationAssertion;
            }
        }
    }

    private static void AssertRootTypeAuthorization(RootType rootType, ValidationOptions options)
    {
        var validations = rootType.Validate().ToArray();
        if (!options.AllowRootTypeWithoutAuthorization && validations.Any(x => x.Type == MissingAuthorization))
        {
            throw new SchemaAuthorizationMissingAuthorization(rootType.Name);
        }

        if (!options.AllowRootTypeEmptyAuthorize && validations.Any(x => x.Type == MissingAuthorizationConstraints))
        {
            throw new SchemaAuthorizationMissingConstraints(rootType.Name); 
        }

        foreach (var field in rootType.Fields)
        {
            AssertFieldAuthorization(field, rootType);
        }
    }

    private static void AssertFieldAuthorization(FieldType field, RootType rootType)
    {
        var validations = field.Validate(rootType).ToArray();

        if (validations.Any(x => x.Type == MissingAuthorization))
        {
            throw new SchemaAuthorizationMissingAuthorization($"{rootType.Name} => {field.Name}");
        }

        if (validations.Any(x => x.Type == MissingAuthorizationConstraints))
        {
          throw new SchemaAuthorizationMissingConstraints($"{rootType.Name} => {field.Name}"); 
        }

        if (validations.Any(x => x.Type == MissingFieldAuthorization))
        {
            throw new SchemaAuthorizationMissingFieldAuthorization($"{rootType.Name} => {field.Name}"); 
        }
    }
}