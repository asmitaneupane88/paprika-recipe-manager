using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    [ObservableProperty] public partial string Name { get; set; }
    
    [ObservableProperty] public partial string ModifierNote { get; set; }
    
    [ObservableProperty] public partial double Quantity { get; set; }
    
    [JsonConverter(typeof(UnitTypeJsonConverter))]
    [ObservableProperty] public partial UnitType Unit { get; set; }
    
    [JsonIgnore]
    public ObservableCollection<UnitType> UnitOptions { get; } = new(Enum.GetValues<UnitType>());
    
    /// <summary>
    /// For use in scaling up recipes like a multiplier
    /// </summary>
    [ObservableProperty] public partial double ScaleFactor { get; set; }
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
