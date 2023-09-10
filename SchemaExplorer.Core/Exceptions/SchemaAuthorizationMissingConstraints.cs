namespace SchemaExplorer.Core.Exceptions;

public sealed class SchemaAuthorizationMissingConstraints : Exception
{
    public SchemaAuthorizationMissingConstraints(string name)
        : base($"The type/field '{name}' is missing constraints like roles/policies.")
    {
    }
}