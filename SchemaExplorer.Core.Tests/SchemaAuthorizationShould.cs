using SchemaExplorer.Core.Exceptions;
using SchemaExplorer.Core.Types;
using static SchemaExplorer.Core.Types.ValidationAssertionType;

namespace SchemaExplorer.Core.Tests;

[TestFixture]
public sealed class SchemaAuthorizationShould
{
    private readonly string _missingRootSdl = File.ReadAllText(
        Path.Combine("schemas", "missing-root-auth-directive.graphql")
    );

    private readonly string _missingFieldRole = File.ReadAllText(
        Path.Combine("schemas", "missing-role-for-field.graphql")
    );

    private readonly string _missingFieldAuthorization = File.ReadAllText(
        Path.Combine("schemas", "missing-field-authorization.graphql")
    );

    private readonly string _missingFieldAuthorizationRootHasConstraint = File.ReadAllText(
        Path.Combine("schemas", "missing-field-authorization-root-has-constraints.graphql")
    );

    private readonly string _noAuthorization = File.ReadAllText(
        Path.Combine("schemas", "no-authorization.graphql")
    );

    [Test]
    public void ExceptionOnMissingRootTypeAuthorizationDirective()
    {
        Assert.Throws<MissingAuthorizationDirectiveException>(
            () =>
                SchemaAuthorization.AssertValidate(
                    _missingRootSdl,
                    new ValidationOptions(AllowRootTypeWithoutAuthorization: false)
                )
        );

        var validations = SchemaAuthorization.Validate(_missingRootSdl).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(
                validations.Any(x => x is { Type: MissingAuthorization, Name: "Mutation" })
            );
            Assert.That(
                validations.Any(x => x is { Type: MissingAuthorization, Name: "Subscription" })
            );
        });
    }

    [Test]
    public void ExceptionWhenRootMissingRoles()
    {
        Assert.Throws<MissingAuthorizationConstraintsException>(
            () =>
                SchemaAuthorization.AssertValidate(
                    _missingRootSdl,
                    new ValidationOptions(AllowRootTypeEmptyAuthorize: false)
                )
        );

        var validations = SchemaAuthorization.Validate(_missingRootSdl).ToArray();
        Assert.That(
            validations.Any(x => x is { Type: MissingAuthorizationConstraints, Name: "Query" })
        );
    }

    [Test]
    public void AllowMissingRootTypeAuthorization()
    {
        Assert.DoesNotThrow(
            () =>
                SchemaAuthorization.AssertValidate(
                    _missingRootSdl,
                    new ValidationOptions(AllowRootTypeWithoutAuthorization: true)
                )
        );
    }

    [Test]
    public void ExceptionOnMissingRoleForField()
    {
        Assert.Throws<MissingAuthorizationConstraintsException>(
            () => SchemaAuthorization.AssertValidate(_missingFieldRole, new ValidationOptions())
        );

        var validations = SchemaAuthorization.Validate(_missingFieldRole).ToArray();
        Assert.That(
            validations.Any(
                x => x is { Type: MissingAuthorizationConstraints, Name: "Query.listItems" }
            )
        );
    }

    [Test]
    public void ExceptionOnMissingFieldAuthorization()
    {
        Assert.Throws<FieldMissingAuthorizationDirectiveException>(
            () =>
                SchemaAuthorization.AssertValidate(
                    _missingFieldAuthorization,
                    new ValidationOptions()
                )
        );

        var validations = SchemaAuthorization.Validate(_missingFieldAuthorization).ToArray();
        Assert.That(
            validations.Any(x => x is { Type: MissingFieldAuthorization, Name: "Query.listItems" })
        );
    }

    [Test]
    public void AllowMissingFieldAuthorization()
    {
        Assert.DoesNotThrow(
            () =>
                SchemaAuthorization.AssertValidate(
                    _missingFieldAuthorizationRootHasConstraint,
                    new ValidationOptions(AllowFieldWithoutAuthorization: true)
                )
        );
    }

    [Test]
    public void ExceptionOnMissingRootAndFieldAuthorization()
    {
        Assert.Throws<MissingAuthorizationDirectiveException>(
            () => SchemaAuthorization.AssertValidate(_noAuthorization)
        );

        var validations = SchemaAuthorization.Validate(_noAuthorization).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(
                validations.Any(x => x is { Type: MissingAuthorization, Name: "Query" }),
                Is.True
            );
            Assert.That(
                validations.Any(
                    x => x is { Type: MissingAuthorization, Name: "Query.currentUser" }
                ),
                Is.True
            );
            Assert.That(
                validations.Any(x => x is { Type: MissingAuthorization, Name: "Query.listItems" }),
                Is.True
            );
        });
    }
}
