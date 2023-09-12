namespace SchemaExplorer.Core.Exceptions;

public sealed class MissingAuthorizationDirectiveException : Exception
{
    public MissingAuthorizationDirectiveException(string name)
        : base($"The type/field '{name}' is missing a authorization directive.") { }
}
