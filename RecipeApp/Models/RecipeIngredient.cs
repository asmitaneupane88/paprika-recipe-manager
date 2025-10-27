using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    public string Name { get; set; }
    
    public string ModifierNote { get; set; }
    
    public double Quantity { get; set; }
    
    [JsonConverter(typeof(UnitTypeJsonConverter))]
    public UnitType Unit { get; set; }
}

// had Claude 4.5 Sonnet quickly generate this converter to fix an issue
public class UnitTypeJsonConverter : JsonConverter<UnitType>
{
    public override UnitType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Enum.TryParse<UnitType>(value, true, out var result) ? result : UnitType.ITEM;
    }

    public override void Write(Utf8JsonWriter writer, UnitType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
