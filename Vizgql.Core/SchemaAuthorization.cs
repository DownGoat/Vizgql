using Vizgql.Core.Exceptions;
using Vizgql.Core.Types;
using static Vizgql.Core.Types.ValidationAssertionType;

namespace Vizgql.Core;

public static class SchemaAuthorization
{
    public static void AssertValidate(string sdl) => AssertValidate(sdl, new ValidationOptions());

    public static void AssertValidate(string sdl, ValidationOptions validationOptions)
    {
        var parser = new SchemaParser();

        var schemaType = SchemaParser.Parse(sdl);

        var exceptions = new List<Exception>();
        foreach (var rootType in schemaType.RootTypes)
        {
            exceptions.AddRange(AssertRootTypeAuthorization(rootType, validationOptions));
        }

        if (exceptions.Any())
            throw new AggregateException(exceptions);
    }

    public static IEnumerable<ValidationAssertion> Validate(string sdl)
    {
        var parser = new SchemaParser();

        var schemaType = SchemaParser.Parse(sdl);

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

    private static List<Exception> AssertRootTypeAuthorization(
        RootType rootType,
        ValidationOptions options
    )
    {
        var validations = rootType.Validate().ToArray();
        var exceptions = new List<Exception>();
        if (
            !options.AllowRootTypeWithoutAuthorization
            && validations.Any(x => x.Type == MissingAuthorization)
        )
        {
            exceptions.Add(new MissingAuthorizationDirectiveException(rootType.Name));
        }

        if (
            !options.AllowRootTypeEmptyAuthorize
            && validations.Any(x => x.Type == MissingAuthorizationConstraints)
        )
        {
            exceptions.Add(new MissingAuthorizationConstraintsException(rootType.Name));
        }

        foreach (var field in rootType.Fields)
        {
            exceptions.AddRange(AssertFieldAuthorization(field, rootType, options));
        }

        return exceptions;
    }

    private static IEnumerable<Exception> AssertFieldAuthorization(
        FieldType field,
        RootType rootType,
        ValidationOptions options
    )
    {
        var validations = field.Validate(rootType).ToArray();

        if (FieldAuthorizationRules.MissingAuthorizationDirective(validations))
        {
            yield return new MissingAuthorizationDirectiveException(rootType.Name, field.Name);
        }

        if (FieldAuthorizationRules.MissingConstraints(validations))
        {
            yield return new MissingAuthorizationConstraintsException(rootType.Name, field.Name);
        }

        if (FieldAuthorizationRules.FieldMissingAuthorization(validations, options))
        {
            yield return new FieldMissingAuthorizationDirectiveException(rootType.Name, field.Name);
        }
    }
}
