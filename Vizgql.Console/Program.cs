// See https://aka.ms/new-console-template for more information

using Vizgql.Core;
using Vizgql.ReportBuilder;

var sdlContent = File.ReadAllText("vortex.graphql");

var schemaType = SchemaParser.Parse(sdlContent);
Console.WriteLine(TextReport.Create(schemaType));