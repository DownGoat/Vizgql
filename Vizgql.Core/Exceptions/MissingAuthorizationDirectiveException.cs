namespace Vizgql.Core.Exceptions;

public sealed class MissingAuthorizationDirectiveException : Exception
{
    public MissingAuthorizationDirectiveException(string rootName)
        : base(
            $"""

             The root type {rootName} is missing authorization directive
             {rootName} -@authorize

             """
        ) { }

    public MissingAuthorizationDirectiveException(string rootName, string fieldName)
        : base(
            $"""

             The {rootName} field '{fieldName}' is missing authorization directive
             {rootName}
                 │
                 └──── {fieldName} -@authorize

             """
        ) { }
}
