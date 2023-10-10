using System.Data;
using System.Text;
using Vizgql.Core.Types;
using SchemaType = Vizgql.Core.Types.SchemaType;

namespace Vizgql.ReportBuilder;

public sealed class ValidationsTextReport
{
    public static string Create(SchemaType schemaType)
    {
        var sb = new StringBuilder();
        
        var validations = schemaType.Validate();

        sb.Append("\nValidations errors:\n");
        foreach (var validationAssertion in validations)
        {
            sb.Append($"{validationAssertion.Name} - {ValidationAssertionTypeDescriptions.ToString(validationAssertion.Type)}\n");
        }
        
        return sb.ToString();
    }
}