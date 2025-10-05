namespace RecipeApp.Models;

// TODO: for use in recipe steps
public record RecipeIngredient
{
    public string Name { get; init; }
    public string Modifier { get; init; }
    
    public double Quantity { get; init; }
    public UnitType Unit { get; init; }
}
