using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RecipeApp.Models;

public class Recipe : INotifyPropertyChanged
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
    public double Rating { get; set; }
    public string? ImageUrl { get; init; }
    public string? Source { get; init; } // "MealDB" or "Local"
    public string? MealDbId { get; init; } // Only for MealDB recipes

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