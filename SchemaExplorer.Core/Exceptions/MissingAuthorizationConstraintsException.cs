namespace SchemaExplorer.Core.Exceptions;

public sealed class MissingAuthorizationConstraintsException : Exception
{
    public MissingAuthorizationConstraintsException(string name)
        : base($"The type/field '{name}' is missing constraints like roles/policies.") { }
}
