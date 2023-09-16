using CommandLine;
using Vizgql.Console;
using Vizgql.Core;
using Vizgql.ReportBuilder;

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
    var textReport = TextReport.Create(schemaType);

    Console.WriteLine(textReport);

    if (!string.IsNullOrEmpty(options.OutputPath))
    {
        File.WriteAllText(options.OutputPath, textReport);
    }
}

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(async options => await HandleOptionsAsync(options, new CancellationToken()));
