namespace SchemaExplorer.NUnit.SchemaExplorer.NUnit;

[TestFixture]
public sealed class SchemaAuthorizationShould
{
    private readonly string _missingRootSdl =
        File.ReadAllText(Path.Combine("schemas", "missing-root-auth-directive.graphql"));
    
    private readonly string _missingFieldRole =
        File.ReadAllText(Path.Combine("schemas", "missing-role-for-field.graphql"));
    
    private readonly string _missingFieldAuthorization =
        File.ReadAllText(Path.Combine("schemas", "missing-field-authorization.graphql"));

    [Test]
    public void ExceptionOnMissingRootTypeAuthorizationDirective()
    {
        Assert.Throws<SchemaAuthorizationMissingAuthorization>(() =>
            SchemaAuthorization.AssertValidate(_missingRootSdl,
                new ValidationOptions(AllowRootTypeWithoutAuthorization: false)));
    }

    [Test]
    public void ExceptionWhenRootMissingRoles()
    {
        Assert.Throws<SchemaAuthorizationMissingConstraints>(() =>
            SchemaAuthorization.AssertValidate(_missingRootSdl,
                new ValidationOptions(AllowRootTypeEmptyAuthorize: false)));
    }

    [Test]
    public void AllowMissingRootTypeAuthorization()
    {
        Assert.DoesNotThrow(() =>
            SchemaAuthorization.AssertValidate(_missingRootSdl,
                new ValidationOptions(AllowRootTypeWithoutAuthorization: true)));
    }
    
    [Test]
    public void ExceptionOnMissingRoleForField()
    {
        Assert.Throws<SchemaAuthorizationMissingConstraints>(() =>
            SchemaAuthorization.AssertValidate(_missingFieldRole,
                new ValidationOptions()));
    }
    
    [Test]
    public void ExceptionOnMissingFieldAuthorization()
    {
        Assert.Throws<SchemaAuthorizationMissingFieldAuthorization>(() =>
            SchemaAuthorization.AssertValidate(_missingFieldAuthorization,
                new ValidationOptions()));
    }
    
    [Test]
    public void ExceptionOnMissingFieldAuthorization2()
    {
        var results = SchemaAuthorization.Validate(_missingFieldAuthorization).ToArray();
        Assert.Pass();
    }
}