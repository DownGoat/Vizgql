// See https://aka.ms/new-console-template for more information

using Vizgql.Core;

var sdlContent = File.ReadAllText("cow.graphql");
var sp = new SchemaParser();

var schemaType = SchemaParser.Parse(sdlContent);
var validations = schemaType.Validate();

SchemaAuthorization.AssertValidate(sdlContent);

Console.WriteLine("asd");
