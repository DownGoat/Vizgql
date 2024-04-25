using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder.Html.Models;

public sealed record TableComponentModel(ConstraintHeader[] ConstraintHeaders, RootTypeGroup[] RootTypes);

public record RootTypeGroup(string Name, bool HasAuthorization, string[] Constraints, TableRow[] Rows);
public record TableRow(string Name, bool HasAuthorization, string[] Constraints);

public record ConstraintHeader(string Name, string Type);
