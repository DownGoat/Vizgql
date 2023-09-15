# Goals
The goal of this project is to have a tool for vizualizing and testing
authorization for GraphQL schemas. It is not always clear and easy to vizualize
how authorization constraints (roles/policies) are applied fields. This
projects aims to give you tools to automatically test this, and genrate vizualisations.

# SchemaExplorer.Core
This package contains the logic for parsing and finding validation errors for schemas.

## Installation
...

## Usage
```SchemaAuthorization.AssertValidate(stringSchema)```

Asserts validation, optional `ValidationOptions` to configure.

```var results = SchemaAuthorization.Validate(stringSchema)``` 

for a list of `ValidationAssertion` objects.

...
more to come