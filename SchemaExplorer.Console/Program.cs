// See https://aka.ms/new-console-template for more information

using SchemaExplorer.Core;

var sdlContent = File.ReadAllText("cow.graphql");
var sp = new SchemaParser();

var schemaType = sp.Parse(sdlContent);
var validations = schemaType.Validate();

Console.WriteLine("asd");
