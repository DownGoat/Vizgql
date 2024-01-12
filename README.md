# Vizgql.Console

Vizgql.Console is a command-line interface tool designed for parsing GraphQL SDL documents. It provides an overview of fields and their associated authorization directives, along with listing different roles/policies the directives may have.

## Features

- Parse GraphQL SDL documents from a file or URL.
- Display fields with authorization directives.
- List roles and policies linked to the authorization directives.
- Highlight validation errors and unique constraints in the schema.

## Installation

`dotnet tool install Vizgql.Console --global`

## Usage

To use Vizgql.Console, execute the following commands with the appropriate options:

```
vizgql -f [path_to_file]
vizgql -u [URL]
```

### Options

- `-f, --file`: Path to the file to be parsed.
- `-u, --url`: URL from which text will be downloaded for parsing.
- `-n, --header-name`: HTTP header name for authentication (Default: Authorization).
- `-t, --header-token`: HTTP header token for authentication.
- `-p, --policies`: Comma-separated list of policies to apply to the schema.
- `-r, --roles`: Comma-separated list of roles to apply to the schema.
- `--validations`: Print out any validation errors (Default: false).
- `--unique-constraints`: Prints all the unique constraints as a comma-separated list.
- `--format`: Ansi, Html, Csv
- `--help`: Display help screen.
- `--version`: Display version information.

### Example

```
PS C:\Users\User> vizgql -u https://hotchocolateschema.com/graphql?sdl --validations --unique-constraints
```
![Output when downloading sdl from a URL. Using the validations and unique-constraints option.](docs/images/example.png "Example output")

### Formats
You can output in several formats
#### Ansi
Default format, for cli usage. Supports validations, unique-constraints, roles and policy filter.
#### Html
Outputs HTML. Supports validations and unique-constraints.
#### Csv
Outputs CSV, only list the fields and constraints.


### In unit tests.
```dotnet add package Vizgql.Core```

```
[Test]

    private readonly ValidationAssertion[] _allowedAssertions =
    [
        new("Mutation", MissingAuthorizationConstraints),
        new("Query", MissingAuthorizationConstraints),
        new("Subscription.priceChange", MissingFieldAuthorization)
    ];
    
    public async Task SchemaChangedAsync()
    {
        var schema = await GraphQLTestService.GetSchema();
        var schemaType = SchemaParser.Parse(schema.ToString());

        var validations = schemaType.Validate().ToArray();

        foreach (var validation in validations)
        {
            Assert.That(_allowedAssertions, Does.Contain(validation));
        }

        Assert.That(validations, Has.Length.EqualTo(_allowedAssertions.Length));
    }
```

Above is a example setup for unit test for C#. `GraphQLTestService` is setup for the HotChocolate GraphQL API. 
`GetSchema` returns the `ISchema` for the API. We can get validations from the parsed `SchemaType` with the `Validations()` method.
You can assert that it should be empty, or make exceptions for known assertions like in the example above.
Not all validation errors are necessary to fix for a secure schema, as authorizations are inherited.