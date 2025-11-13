using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    public RecipeIngredient()
    {
        Category = "Uncategorized";
    }

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
            "Others"
        };
    
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

