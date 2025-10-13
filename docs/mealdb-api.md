# Meal DB API Integration (Search Feature)

## Overview
Implements live recipe search using [TheMealDB API](https://www.themealdb.com/).

Developed by **Hoang My Le**, this feature connects the UI, ViewModel, and API service to fetch and display meal data dynamically.

## Architecture
**Flow:**
1. User enters query in the `SearchPage`.
2. `SearchViewModel` calls `_recipeService.SearchAsync(query)`.
3. `ApiControl` sends HTTP request to TheMealDB API.
4. Response JSON → `MealDbRecipe` model → Displayed on UI via data binding.

## File involved
| File | Role |
|------|------|
| `Models/MealDbRecipe.cs` | Maps API response data |
| `Services/IRecipeService.cs` | Defines contract for recipe search |
| `Services/ApiControl.cs` | Implements API logic |
| `ViewModels/SearchViewModel.cs` | Handles command, state, and data binding |
| `Controls/Pages/SearchPage.xaml` | UI for user search and recipe list |

## Example Code
```csharp
// SearchViewModel.cs
var results = await _recipeService.SearchAsync(Query);
Recipes.Clear();
foreach (var r in results)
    Recipes.Add(r);
