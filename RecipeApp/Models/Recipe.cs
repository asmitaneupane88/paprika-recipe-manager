using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RecipeApp.Enums;
namespace RecipeApp.Models;

public class Recipe : INotifyPropertyChanged, IRecipe
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Title { get; init; }
    public string? Category { get; init; }
    public string? Author { get; init; }
    public required int PrepTimeMinutes { get; init; }
    public required int CookTimeMinutes { get; init; }
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
    public required int Servings { get; init; }
    public required string Difficulty { get; init; }
    public required ObservableCollection<Ingredient> Ingredients { get; init; } = [];
    public required ObservableCollection<string> Directions { get; init; } = [];
    public int Rating { get; set; }
    public string? ImageUrl { get; init; }
    public string? Source { get; init; } // "MealDB" or "Local"
    public string? MealDbId { get; init; } // Only for MealDB recipes
    public string? Description { get; init; }

    // converts Ingredient -> RecipeIngredient
    ObservableCollection<RecipeIngredient> IRecipe.Ingredients
    {
        get
        {
            var result = new ObservableCollection<RecipeIngredient>();
            foreach (var ingredient in Ingredients)
            {
                double quantity = 1;
                if (!string.IsNullOrWhiteSpace(ingredient.Amount) &&
                     double.TryParse(ingredient.Amount, out var parsed))
                {
                    quantity = parsed;
                }

                result.Add(new RecipeIngredient {
                    Name = ingredient.Name ?? string.Empty,
                    Quantity = quantity,
                    Unit = ParseUnitType(ingredient.Unit),
                    ModifierNote = ingredient.Notes ?? string.Empty
                });
            }
            return result;
        }
    }

    private static UnitType ParseUnitType(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return UnitType.ITEM;
    
        return unit.ToUpperInvariant().Trim() switch
        {
            "TSP" or "TEASPOON" or "TEASPOONS" => UnitType.TSP,
            "TBSP" or "TABLESPOON" or "TABLESPOONS" => UnitType.TBSP,
            "CUP" or "CUPS" => UnitType.CUP,
            "PINT" or "PINTS" => UnitType.PINT,
            "QUART" or "QUARTS" => UnitType.QUART,
            "GALLON" or "GALLONS" => UnitType.GALLON,
            "OZ" or "OUNCE" or "OUNCES" => UnitType.OZ,
            "LB" or "LBS" or "POUND" or "POUNDS" => UnitType.LB,
            "KG" or "KILOGRAM" or "KILOGRAMS" => UnitType.KG,
            _ => UnitType.ITEM
        };
}
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Ingredient : INotifyPropertyChanged
{
    private string? _amount;
    private string? _unit;
    private string _name = string.Empty;
    private string? _notes;

    public string? Amount 
    { 
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
    }

    public string? Unit
    {
        get => _unit;
        set
        {
            if (_unit != value)
            {
                _unit = value;
                OnPropertyChanged();
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public string? Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
