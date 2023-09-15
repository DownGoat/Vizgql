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
        const string exceptionName = nameof(MissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(
                _missingRootSdl,
                new ValidationOptions(AllowRootTypeWithoutAuthorization: false)
            );
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

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
        const string exceptionName = nameof(MissingAuthorizationConstraintsException);
        try
        {
            SchemaAuthorization.AssertValidate(
                _missingRootSdl,
                new ValidationOptions(AllowRootTypeEmptyAuthorize: false)
            );
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

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
        const string exceptionName = nameof(MissingAuthorizationConstraintsException);
        try
        {
            SchemaAuthorization.AssertValidate(_missingFieldRole, new ValidationOptions());
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

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
        const string exceptionName = nameof(FieldMissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(_missingFieldAuthorization, new ValidationOptions());
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

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
        const string exceptionName = nameof(MissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(_noAuthorization);
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

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
