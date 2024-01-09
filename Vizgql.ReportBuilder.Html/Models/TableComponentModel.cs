using Vizgql.Core.Types;

namespace Vizgql.ReportBuilder.Html.Models;

public sealed record TableComponentModel(ConstraintHeader[] ConstraintHeaders, TableRow[] Rows);

public record TableRow(string Name, bool HasAuthorization, string[] Constraints);

public record ConstraintHeader(string Name, string Type);
