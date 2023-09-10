// See https://aka.ms/new-console-template for more information

using SchemaExplorer.Core;

var sdlContent = File.ReadAllText("cow.graphql");
var sp = new SchemaParser();

sp.Parse(sdlContent);