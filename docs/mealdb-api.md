# Meal DB API Integration (Search Feature)

## Overview
Implements live recipe search using [TheMealDB API](https://www.themealdb.com/).

This feature connects the UI, ViewModel, and API service to fetch and display meal data dynamically.

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

5. **Configure API Connectivity**
- **Purpose**: Ensure the application can connect to the **MealDB** API for recipe search functionality (handled by `Services/ApiControl.cs` and `ViewModels/SearchViewModel.cs`)
- **Steps**:
  1. **Verify Internet Access**:
     - Ensure a stable internet connection, as the MealDB API requires online access.
     - Test connectivity by pinging the API endpoint:
  ```bash
     ping www.themealdb.com
  ```

## Check API Configuration:
- The MealDB API's free tier does not required an API key, and the current implementation hardcodes the API URL in `Services/ApiControl.cs`.
## Test API Connectivity:
- Run the application (`dotnet run --project ReceipeApp/RecipeApp.csproj`)
- Navigate to the search page (`Controls/Pages/MealDbSearchPage`) and perform a test search (e.g., "chicken")
- Verify that `SearchViewModel.cs` polulates `SearchResults` with data from `MealDBRecipe.cs`.
