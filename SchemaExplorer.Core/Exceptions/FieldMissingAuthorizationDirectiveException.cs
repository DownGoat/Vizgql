namespace SchemaExplorer.Core.Exceptions;

public sealed class FieldMissingAuthorizationDirectiveException : Exception
{
    public FieldMissingAuthorizationDirectiveException(string fieldName)
        : base($"The field '{fieldName}' is missing authorization.") { }
}
