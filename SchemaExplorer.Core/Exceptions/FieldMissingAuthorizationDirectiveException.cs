namespace SchemaExplorer.Core.Exceptions;

public sealed class FieldMissingAuthorizationDirectiveException : Exception
{
    public FieldMissingAuthorizationDirectiveException(string rootName, string fieldName)
        : base(
            $"""

The {rootName} field '{fieldName}' is missing authorization directive
{rootName}
    │
    └──── {fieldName} -@authorize

"""
        ) { }
}
