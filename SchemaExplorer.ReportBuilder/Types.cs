namespace SchemaExplorer.ReportBuilder;

public record RootType(string Name, bool HasAuthorization, string[] Roles, FieldType[] Resolvers);
public record FieldType(string Name, bool HasAuthorization, string[] Roles);