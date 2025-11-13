using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    [ObservableProperty] public partial string Name { get; set; }
    
    [ObservableProperty] public partial string ModifierNote { get; set; }
    
    [ObservableProperty] public partial double Quantity { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [ObservableProperty] public partial UnitType Unit { get; set; }
    
    [JsonIgnore]
    public ObservableCollection<UnitType> RecipeUnitOptions { get; } = new(Enum.GetValues<UnitType>());
    
    
    private static readonly List<UnitType> UnitOptions =
    [
        UnitType.Box,
        UnitType.LB,
        UnitType.Gallon,
        UnitType.Quart,
        UnitType.Pint
        //TODO
    ];
    
    [JsonIgnore]
    public ObservableCollection<UnitType> GroceryUnitOptions { get; } = new(Enum.GetValues<UnitType>().Where(ut => UnitOptions.Contains(ut)));
    /// <summary>
    /// For use in scaling up recipes like a multiplier
    /// </summary>
    [ObservableProperty] public partial double ScaleFactor { get; set; }
}

