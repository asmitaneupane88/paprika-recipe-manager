# Pantry Category Grouping

## Overview
Pantry items are grouped by category in the Pantry view to improve navigation and speed up
ingredient lookup. Categories include:
- Vegetables
- Fruits
- Dairy
- Meat
- Seafood
- Snacks
- Baking
- Beverages
- Others
- Uncategorized (default)
## Key Components
### Model Changes (`PantryIngredient.cs`)
```csharp
[ObservableProperty]
public partial string Category { get; set; } = "Uncategorized";

[JsonIgnore]
public ObservableCollection<string> CategoryOptions { get; } =
[
    "Vegetables", "Fruits", "Dairy", "Meat", "Seafood",
    "Baking", "Beverages", "Snacks", "Others", "Uncategorized"
];
```
## Grouping Logic
Implemented in `PantryIngredientsPage.xaml.cs`
```csharp
var grouped = allItems
    .GroupBy(i => string.IsNullOrWhiteSpace(i.PIngredient.Category)
        ? "Uncategorized"
        : i.PIngredient.Category)
    .Select(g => new Grouping<string, IngredientCard>(g.Key, g));
```
Groups are bound to the XAML UI via:
```xaml
ItemsSource="{Binding Source={StaticResource GroupedPantrySource}}"
```
## Filtering
The user can filter items using a dropdown above the list.

Filtering handler:
```csharp
private async void CategoryFilterBox_SelectionChanged(...)
{
    if (selected == "All Categories")
        await ShowIngredients();
    else
        ApplyFilteredGrouping();
}
```


