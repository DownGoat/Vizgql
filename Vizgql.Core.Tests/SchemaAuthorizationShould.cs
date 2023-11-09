using Vizgql.Core.Exceptions;
using Vizgql.Core.Types;
using static Vizgql.Core.Types.ValidationAssertionType;

namespace Vizgql.Core.Tests;

[TestFixture]
public sealed class SchemaAuthorizationShould
{
    private const string Schemas = "schemas";
    private const string RolesDir = "roles";
    private const string PoliciesDir = "policies";
    private const string MissingRootAuthDirectiveSchema = "missing-root-auth-directive.graphql";
    private const string MissingRoleForFieldSchema = "missing-role-for-field.graphql";
    private const string MissingFieldAuthorizationSchema = "missing-field-authorization.graphql";
    private const string SpellingMistakeSchema = "spelling-mistake-constraint.graphql";

    private const string MissingFieldAuthorizationRootHasConstraintsSchema =
        "missing-field-authorization-root-has-constraints.graphql";

    private const string NoAuthorizationSchema = "no-authorization.graphql";

    private readonly Dictionary<string, Dictionary<string, string>> SchemasByDir =
        new()
        {
            {
                RolesDir,
                new Dictionary<string, string>
                {
                    {
                        MissingRootAuthDirectiveSchema,
                        File.ReadAllText(
                            Path.Combine(Schemas, RolesDir, MissingRootAuthDirectiveSchema)
                        )
                    },
                    {
                        MissingRoleForFieldSchema,
                        File.ReadAllText(Path.Combine(Schemas, RolesDir, MissingRoleForFieldSchema))
                    },
                    {
                        MissingFieldAuthorizationSchema,
                        File.ReadAllText(
                            Path.Combine(Schemas, RolesDir, MissingFieldAuthorizationSchema)
                        )
                    },
                    {
                        MissingFieldAuthorizationRootHasConstraintsSchema,
                        File.ReadAllText(
                            Path.Combine(
                                Schemas,
                                RolesDir,
                                MissingFieldAuthorizationRootHasConstraintsSchema
                            )
                        )
                    },
                    {
                        NoAuthorizationSchema,
                        File.ReadAllText(Path.Combine(Schemas, RolesDir, NoAuthorizationSchema))
                    },
                    {
                        SpellingMistakeSchema,
                        File.ReadAllText(Path.Combine(Schemas, RolesDir, SpellingMistakeSchema))
                    }
                }
            },
            {
                PoliciesDir,
                new Dictionary<string, string>
                {
                    {
                        MissingRootAuthDirectiveSchema,
                        File.ReadAllText(
                            Path.Combine(Schemas, PoliciesDir, MissingRootAuthDirectiveSchema)
                        )
                    },
                    {
                        MissingRoleForFieldSchema,
                        File.ReadAllText(
                            Path.Combine(Schemas, PoliciesDir, MissingRoleForFieldSchema)
                        )
                    },
                    {
                        MissingFieldAuthorizationSchema,
                        File.ReadAllText(
                            Path.Combine(Schemas, PoliciesDir, MissingFieldAuthorizationSchema)
                        )
                    },
                    {
                        MissingFieldAuthorizationRootHasConstraintsSchema,
                        File.ReadAllText(
                            Path.Combine(
                                Schemas,
                                PoliciesDir,
                                MissingFieldAuthorizationRootHasConstraintsSchema
                            )
                        )
                    },
                    {
                        NoAuthorizationSchema,
                        File.ReadAllText(Path.Combine(Schemas, PoliciesDir, NoAuthorizationSchema))
                    },
                    {
                        SpellingMistakeSchema,
                        File.ReadAllText(Path.Combine(Schemas, PoliciesDir, SpellingMistakeSchema))
                    }
                }
            }
        };

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void ExceptionOnMissingRootTypeAuthorizationDirective(string dir)
    {
        var schema = SchemasByDir[dir][MissingRootAuthDirectiveSchema];
        const string exceptionName = nameof(MissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(
                schema,
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

        var validations = SchemaAuthorization.Validate(schema).ToArray();
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

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void ExceptionWhenRootMissingRoles(string dir)
    {
        var schema = SchemasByDir[dir][MissingRootAuthDirectiveSchema];
        const string exceptionName = nameof(MissingAuthorizationConstraintsException);
        try
        {
            SchemaAuthorization.AssertValidate(
                schema,
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

        var validations = SchemaAuthorization.Validate(schema).ToArray();
        Assert.That(
            validations.Any(x => x is { Type: MissingAuthorizationConstraints, Name: "Query" })
        );
    }

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void AllowMissingRootTypeAuthorization(string dir)
    {
        var schema = SchemasByDir[dir][MissingRootAuthDirectiveSchema];
        Assert.DoesNotThrow(
            () =>
                SchemaAuthorization.AssertValidate(
                    schema,
                    new ValidationOptions(AllowRootTypeWithoutAuthorization: true)
                )
        );
    }

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void ExceptionOnMissingRoleForField(string dir)
    {
        var schema = SchemasByDir[dir][MissingRoleForFieldSchema];
        const string exceptionName = nameof(MissingAuthorizationConstraintsException);
        try
        {
            SchemaAuthorization.AssertValidate(schema, new ValidationOptions());
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

        var validations = SchemaAuthorization.Validate(schema).ToArray();
        Assert.That(
            validations.Any(
                x => x is { Type: MissingAuthorizationConstraints, Name: "Query.listItems" }
            )
        );
    }

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void ExceptionOnMissingFieldAuthorization(string dir)
    {
        var schema = SchemasByDir[dir][MissingFieldAuthorizationSchema];
        const string exceptionName = nameof(FieldMissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(schema, new ValidationOptions());
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

        var validations = SchemaAuthorization.Validate(schema).ToArray();
        Assert.That(
            validations.Any(x => x is { Type: MissingFieldAuthorization, Name: "Query.listItems" })
        );
    }

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void AllowMissingFieldAuthorization(string dir)
    {
        var schema = SchemasByDir[dir][MissingFieldAuthorizationRootHasConstraintsSchema];
        Assert.DoesNotThrow(
            () =>
                SchemaAuthorization.AssertValidate(
                    schema,
                    new ValidationOptions(AllowFieldWithoutAuthorization: true)
                )
        );
    }

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void ExceptionOnMissingRootAndFieldAuthorization(string dir)
    {
        var schema = SchemasByDir[dir][NoAuthorizationSchema];
        const string exceptionName = nameof(MissingAuthorizationDirectiveException);
        try
        {
            SchemaAuthorization.AssertValidate(schema);
            Assert.Fail(
                $"Should have thrown a 'AggregateException' with an inner '{exceptionName}' exception."
            );
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerExceptions.Any(x => x.GetType().Name == exceptionName), Is.True);
        }

        var validations = SchemaAuthorization.Validate(schema).ToArray();
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

    [TestCase(RolesDir)]
    [TestCase(PoliciesDir)]
    public void DetectPotentialSpellingMistake(string dir)
    {
        var schema = SchemasByDir[dir][SpellingMistakeSchema];

        var schemaType = SchemaParser.Parse(schema);
        var validations = schemaType.Validate();

        Assert.That(validations.All(x => x.Type == ConstraintSpellingMistake), Is.EqualTo(true));
    }
}
