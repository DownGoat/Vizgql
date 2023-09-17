# Goals
The goal of this project is to have a tool for vizualizing and testing authorization for GraphQL schemas. It is not always clear and easy to vizualize how authorization constraints (roles/policies) are applied fields. This projects aims to give you tools to automatically test this, and genrate vizualisations.

# Vizgql.Core
This package contains the logic for parsing and finding validation errors for schemas.

## Installation
`dotnet add package Vizgql.Core`

## In unit tests
We can assert validation of a schema with the `SchemaAuthorization` class. It exposes `AssertValidate` which validates the schema sdl. You can configure some of it behaviour by also passing a `ValidationOptions` object. `AssertValidate` will throw exceptions if the schema fails some validation check. The package is not specific to any testing framework. 

Example test
```
[Test]
public void ExampleTest()
{
    SchemaAuthorization.AssertValidate(
                sdlSchema,
                new ValidationOptions(AllowFieldWithoutAuthorization: true)
            );
}
```
## Other
You can use the `Validate` method to find all warnings for the schema. 

## ValidationOptions
```
public sealed record ValidationOptions(
    bool AllowRootTypeWithoutAuthorization = false,
    bool AllowRootTypeEmptyAuthorize = true,
    bool AllowFieldWithoutAuthorization = false
);
```

### AllowRootTypeWithoutAuthorization
If this is enabled, validation will pass for missing `@authorize` directive on root types such as `Query` and `Mutation`.

### AllowRootTypeEmptyAuthorize
If enabled validation will pass for missing constraints such as roles/policies on root types such as `Query` and `Mutation`.

### AllowFieldWithoutAuthorization
If enabled validation will pass for missing `@authorize` directive on fields.

