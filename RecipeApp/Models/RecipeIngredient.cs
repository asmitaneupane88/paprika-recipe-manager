namespace RecipeApp.Models;

// TODO: for use in recipe steps
public partial class RecipeIngredient : IAutosavingClass<RecipeIngredient>
{
    public string Name { get; init; }
    
    public string ModifierNote { get; init; }
    
    public double Quantity { get; init; }
    public UnitType Unit { get; init; }
}
