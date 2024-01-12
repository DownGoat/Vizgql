namespace Vizgql.ReportBuilder.Html.Models;

public sealed record HtmlReportComponentModel(
    TableComponentModel TableComponentModel,
    UniqueConstraintsComponentModel? UniqueConstraintsComponentModel,
    ValidationsComponentModel? ValidationsComponentModel,
    HtmlReportComponentOptions Options);

public sealed record HtmlReportComponentOptions(bool ShowUniqueConstraints, bool ShowValidations);