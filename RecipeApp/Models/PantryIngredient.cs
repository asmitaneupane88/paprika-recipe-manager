using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class PantryIngredient: IAutosavingClass<PantryIngredient>
{
    [ObservableProperty] public partial string Name { get; set; }
    
    [ObservableProperty] public partial string ModifierNote { get; set; }
    
    [ObservableProperty] public partial double Quantity { get; set; }
    
    [JsonConverter(typeof(UnitTypeJsonConverter))]
    [ObservableProperty] public partial UnitType Unit { get; set; }
    
    [ObservableProperty] public partial string Category { get; set; } = "Uncategorized";
    
    [JsonIgnore]
    public ObservableCollection<string> CategoryOptions { get; } =
        new()
        {
            "Vegetables",
            "Fruits",
            "Dairy",
            "Meat",
            "Seafood",
            "Baking",
            "Beverages",
            "Snacks",
            "Chicken",
            "Pasta",
            "Others"
        };
    
    
    private static readonly List<UnitType> UnitOptions = new()
    {
        UnitType.Box,
        UnitType.LB,
        UnitType.Gallon,
        UnitType.Quart,
        UnitType.Pint
        // TODO: extend as needed
    };
    
    [JsonIgnore]
    public ObservableCollection<UnitType> PantryUnitOptions { get; } =
        new(Enum.GetValues<UnitType>().Where(UnitOptions.Contains));

    /// <summary>
    /// For use in scaling up recipes like a multiplier
    /// </summary>
    [ObservableProperty]
    public partial double ScaleFactor { get; set; } = 1.0;
}
