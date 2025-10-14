### Recipe Model

The `Recipe` class serves as the primary data model for recipes in the application. It implements both `INotifyPropertyChanged` for UI updates and `IRecipe` for standard recipe functionality.

#### Key Properties

- `Title`: The name of the recipe (required)
- `Description`: Detailed recipe instructions or notes
- `Category`: Recipe classification (e.g., "Dinner", "Dessert")
- `PrepTimeMinutes`: Preparation time in minutes
- `CookTimeMinutes`: Cooking time in minutes
- `TotalTimeMinutes`: Automatically calculated total time (prep + cook)
- `Servings`: Number of portions the recipe yields
- `Ingredients`: Collection of recipe ingredients
- `Directions`: Step-by-step cooking instructions
- `Rating`: User-assigned rating (0-5 stars)
- `ImageUrl`: URL to recipe image
- `Source`: Origin of the recipe ("MealDB" or "Local")
- `MealDbId`: External ID for recipes from MealDB

### EditRecipe View

The EditRecipe view provides a user interface for creating and modifying recipes.

#### Features

- Recipe title editing
- Description input with multi-line support
- Category assignment
- Rating assignment (star-based)
- Image URL input
- Ingredient management
  - Add/remove ingredients
  - Specify quantities and units
- Step-by-step instruction editing
- Save and cancel operations

### RecipeDetails View

The RecipeDetails view displays comprehensive recipe information.

#### Features

- Recipe header with title and category
- Preparation and cooking time display
- Servings information
- Ingredient list with quantities
- Step-by-step cooking instructions
- Rating display
- Recipe image display
- Source attribution

## Data Flow

1. Recipe data is loaded from either local storage or the MealDB API
2. Data is mapped to the Recipe model
3. Users can view details in RecipeDetails view
4. Users can modify recipes in EditRecipe view
5. Changes are saved back to local storage

## Example Usage

### Creating a New Recipe

```csharp
var newRecipe = new Recipe
{
    Title = "Chocolate Cake",
    PrepTimeMinutes = 15,
    CookTimeMinutes = 30,
    Servings = 8,
    Difficulty = "Medium",
    Category = "Dessert"
};
```
