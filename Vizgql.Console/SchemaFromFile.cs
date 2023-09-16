namespace Vizgql.Console;

public static class SchemaFromFile
{
    public static string Read(string path)
    {
        return File.ReadAllText(path);
    }
}
