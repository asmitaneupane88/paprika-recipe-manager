using System.Text.Json.Serialization;

namespace RecipeApp.Enums;

//TODO: verify that these are all of them and not too many
public enum UnitType
{
    Box,
    Tsp,
    Tbsp,
    Cup,
    Pint,
    Quart,
    Gallon,
    OZ,
    LB,
    KG,
}

public partial class UnitTypeJsonConverter : JsonConverter<UnitType>
{
    public override UnitType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (Enum.TryParse<UnitType>(stringValue, ignoreCase: true, out var result))
            return result;
        
        return UnitType.Box;
    }

    public override void Write(Utf8JsonWriter writer, UnitType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
