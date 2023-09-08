namespace SchemaExplorer.NUnit.Exceptions;

public sealed class SchemaAuthorizationMissingFieldAuthorization : Exception
{
    public SchemaAuthorizationMissingFieldAuthorization(string fieldName)
        : base($"The field '{fieldName}' is missing authorization.")
    {
    }
}