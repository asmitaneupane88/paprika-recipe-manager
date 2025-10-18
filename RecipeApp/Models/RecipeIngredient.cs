using RecipeApp.Enums;

namespace RecipeApp.Models;

public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    public string Name { get; set; }
    
    public string ModifierNote { get; set; }
    
    public double Quantity { get; set; }
    public UnitType Unit { get; set; }
}
