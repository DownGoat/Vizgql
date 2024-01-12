using CommandLine;
using Vizgql.Console;
using Vizgql.Core;
using Vizgql.ReportBuilder;
using Vizgql.ReportBuilder.Html.Models;

static async Task HandleOptionsAsync(
    CommandLineOptions options,
    CancellationToken cancellationToken
)
{
    string textToParse;

    if (!string.IsNullOrEmpty(options.InputPath))
    {
        textToParse = SchemaFromFile.Read(options.InputPath);
    }
    else if (!string.IsNullOrEmpty(options.Url))
    {
        textToParse = await SchemaFromHttp.ReadAsync(options, cancellationToken);
    }
    else
    {
        Console.WriteLine("Either file path or URL must be provided.");
        return;
    }

    var schemaType = SchemaParser.Parse(textToParse);

    switch (options.OutputFormat)
    {
        case OutputFormat.Ansi:
            SchemaTextReport.Create(
                schemaType,
                new SchemaTextReportOptions(
                    options.Validations,
                    options.GetRoles() ?? Array.Empty<string>(),
                    options.GetPolicies() ?? Array.Empty<string>(),
                    options.UniqueConstraints
                )
            );
            break;
        case OutputFormat.Csv:
            CsvReport.Create(schemaType);
            break;
        case OutputFormat.Html:
            HtmlReport.Create(schemaType,
                new HtmlReportComponentOptions(options.UniqueConstraints, options.Validations));
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(async options => await HandleOptionsAsync(options, new CancellationToken()));