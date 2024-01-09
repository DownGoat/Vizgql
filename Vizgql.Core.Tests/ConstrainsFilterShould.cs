using Vizgql.Core.Types;

namespace Vizgql.Core.Tests;

[TestFixture]
public sealed class ConstrainsFilterShould
{
    [Test]
    public void FindMissingRootTypeConstraint()
    {
        var rolesPath = Path.Combine("schemas", "roles", "constraint-filter-test.graphql");
        var policiesPath = Path.Combine("schemas", "policies", "constraint-filter-test.graphql");


        var rolesSdl = File.ReadAllText(rolesPath);
        var rolesSchemaType = SchemaParser.Parse(rolesSdl);

        var policiesSdl = File.ReadAllText(policiesPath);
        var policiesSchema = SchemaParser.Parse(policiesSdl);

        var constraints = new Constraints(["Core"], ["Core"]);
        
        var rolesResult = ConstraintsFilter.GetNotAuthorizedTypes(rolesSchemaType, constraints);
        var policiesResult = ConstraintsFilter.GetNotAuthorizedTypes(policiesSchema, constraints);
        
        Assert.Multiple(() =>
        {
            Assert.That(rolesResult.NotAuthorizedFieldsByRootType, Has.Count.EqualTo(1));
            Assert.That(rolesResult.NotAuthorizedFieldsByRootType.Values.All(x => x.All(y => y.Name == "listItems")), Is.True);
            Assert.That(rolesResult.NotAuthorizedRootTypes, Has.Count.EqualTo(1));
            Assert.That(rolesResult.NotAuthorizedRootTypes.First().Name, Is.EqualTo("Mutation"));
        });
        
        Assert.Multiple(() =>
        {
            Assert.That(policiesResult.NotAuthorizedFieldsByRootType, Has.Count.EqualTo(1));
            Assert.That(policiesResult.NotAuthorizedFieldsByRootType.Values.All(x => x.All(y => y.Name == "listItems")), Is.True);
            Assert.That(policiesResult.NotAuthorizedRootTypes, Has.Count.EqualTo(1));
            Assert.That(policiesResult.NotAuthorizedRootTypes.First().Name, Is.EqualTo("Mutation"));
        });
    }
}