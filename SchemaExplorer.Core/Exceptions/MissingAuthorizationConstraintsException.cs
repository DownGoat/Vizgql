namespace SchemaExplorer.Core.Exceptions;

public sealed class MissingAuthorizationConstraintsException : Exception
{
    public MissingAuthorizationConstraintsException(string rootName)
        : base(
            $"""

             The root type '{rootName}' is missing constraints like roles/policies.
             {rootName} -@authorize(roles: ["Foo"] || policies: ["Bar"])

             """
        ) { }

    public MissingAuthorizationConstraintsException(string rootName, string fieldName)
        : base(
            $"""

             The root type '{rootName}' is missing constraints like roles/policies.
             {rootName}
                 │
                 └──── {fieldName} -@authorize(roles: ["Foo"] || policies: ["Bar"])

             """
        ) { }
}
