using SchemaExplorer.ReportBuilder;

namespace SchemaExplorer.NUnit.SchemaExplorer.NUnit;

[TestFixture]
public sealed class SchemaAuthorizationShould
{
    private readonly string _missingRootPath = Path.Combine("schemas", "missing-root-auth-directive.graphql");

    [Test]
    public void ExceptionOnMissingRootTypeAuthorizationDirective()
    {
        var types = GetMissingRootTypes();

        var rootType = types.First(x => x.Name == "Mutation");

        Assert.Throws<SchemaAuthorizationMissingRootDirective>(() =>
            SchemaAuthorization.AssertRootTypeAuthorization(rootType, new ValidationOptions()));
    }

    private RootType[] GetMissingRootTypes()
    {
        var sdl = File.ReadAllText(_missingRootPath);
        var parser = new SchemaParser();
        var types = parser.Parse(sdl);
        return types;
    }

    [Test]
    public void NotThrowExceptionOnRootAuthorized()
    {
        var types = GetMissingRootTypes();

        var rootType = types.First(x => x.Name == "Query");

        Assert.DoesNotThrow(() => SchemaAuthorization.AssertRootTypeAuthorization(rootType, new ValidationOptions()));
    }

    [Test]
    public void ExceptionWhenRootMissingRoles()
    {
        var types = GetMissingRootTypes();

        var rootType = types.First(x => x.Name == "Query");

        Assert.Throws<SchemaAuthorizationMissingRootRoles>(() =>
            SchemaAuthorization.AssertRootTypeAuthorization(rootType,
                new ValidationOptions(AllowRootTypeEmptyAuthorize: false)));
    }

    [Test]
    public void AllowMissingRootTypeAuthorization()
    {
        var types = GetMissingRootTypes();

        var rootType = types.First(x => x.Name == "Mutation");

        Assert.DoesNotThrow(() =>
            SchemaAuthorization.AssertRootTypeAuthorization(rootType,
                new ValidationOptions(AllowRootTypeWithoutAuthorization: true)));
    }
}