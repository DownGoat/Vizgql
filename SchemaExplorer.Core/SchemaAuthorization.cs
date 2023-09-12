using SchemaExplorer.Core.Exceptions;
using SchemaExplorer.Core.Types;
using static SchemaExplorer.Core.Types.ValidationAssertionType;

namespace SchemaExplorer.Core;

public static class SchemaAuthorization
{
    public static void AssertValidate(string sdl) => AssertValidate(sdl, new ValidationOptions());

    public static void AssertValidate(string sdl, ValidationOptions validationOptions)
    {
        var parser = new SchemaParser();

        var schemaType = parser.Parse(sdl);

        foreach (var rootType in schemaType.RootTypes)
        {
            AssertRootTypeAuthorization(rootType, validationOptions);
        }
    }

    public static IEnumerable<ValidationAssertion> Validate(string sdl)
    {
        var parser = new SchemaParser();

        var schemaType = parser.Parse(sdl);

        var validations = new List<ValidationAssertion>();

        foreach (var rootType in schemaType.RootTypes)
        {
            validations.AddRange(rootType.Validate());
            foreach (var field in rootType.Fields)
            {
                validations.AddRange(field.Validate(rootType));
            }
        }

        return validations;
    }

    private static void AssertRootTypeAuthorization(RootType rootType, ValidationOptions options)
    {
        var validations = rootType.Validate().ToArray();
        if (
            !options.AllowRootTypeWithoutAuthorization
            && validations.Any(x => x.Type == MissingAuthorization)
        )
        {
            throw new MissingAuthorizationDirectiveException(rootType.Name);
        }

        if (
            !options.AllowRootTypeEmptyAuthorize
            && validations.Any(x => x.Type == MissingAuthorizationConstraints)
        )
        {
            throw new MissingAuthorizationConstraintsException(rootType.Name);
        }

        foreach (var field in rootType.Fields)
        {
            AssertFieldAuthorization(field, rootType, options);
        }
    }

    private static void AssertFieldAuthorization(
        FieldType field,
        RootType rootType,
        ValidationOptions options
    )
    {
        var validations = field.Validate(rootType).ToArray();
        var fieldName = $"{rootType.Name} => {field.Name}";

        if (FieldAuthorizationRules.MissingAuthorizationDirective(validations))
        {
            throw new MissingAuthorizationDirectiveException(fieldName);
        }

        if (FieldAuthorizationRules.MissingConstraints(validations))
        {
            throw new MissingAuthorizationConstraintsException(fieldName);
        }

        if (FieldAuthorizationRules.FieldMissingAuthorization(validations, options))
        {
            throw new FieldMissingAuthorizationDirectiveException(fieldName);
        }
    }
}
