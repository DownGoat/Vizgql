namespace SchemaExplorer.Core.Exceptions;

public sealed class SchemaAuthorizationMissingAuthorization : Exception
{
    public SchemaAuthorizationMissingAuthorization(string name)
        : base($"The type/field '{name}' is missing a authorization directive.") { }
}
