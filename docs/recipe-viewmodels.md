# Recipe ViewModels Documentation

## RecipeDetailsViewModel

### Overview

The RecipeDetailsViewModel serves as the bridge between the Recipe model and the RecipeDetails view, handling data transformation, business logic, and user interactions.

### Class Definition

```csharp
public class RecipeDetailsViewModel : INotifyPropertyChanged
{
    private readonly Recipe _recipe;

    public RecipeDetailsViewModel(Recipe recipe)
    {
        _recipe = recipe;
    }
}
```

### Key Features

#### 1. Recipe Data Management

- Handles conversion between API recipes and saved recipes
- Manages recipe property updates
- Ensures data consistency

#### 2. Save Operation

```csharp
public async Task<SavedRecipe> SaveRecipeAsync()
{
    // Category management
    // Data conversion
    // Storage operations
}
```

#### 3. Data Transformation

- Converts between different measurement units
- Formats recipe steps
- Handles image URLs

### Property Mappings

| Model Property  | View Property   | Transformation     |
| --------------- | --------------- | ------------------ |
| Title           | Title           | Direct             |
| Description     | Description     | Formatted text     |
| Category        | Category        | Trimmed text       |
| PrepTimeMinutes | PrepTime        | Formatted duration |
| CookTimeMinutes | CookTime        | Formatted duration |
| Ingredients     | IngredientsList | Converted units    |
| Directions      | StepsList       | Numbered steps     |

### Data Flow

1. **Loading Recipe**

   ```csharp
   // Initialize from API recipe
   var viewModel = new RecipeDetailsViewModel(apiRecipe);

   // Initialize from saved recipe
   var viewModel = new RecipeDetailsViewModel(savedRecipe);
   ```

2. **Saving Recipe**
   ```csharp
   // Convert and save
   await viewModel.SaveRecipeAsync();
   ```

### Error Handling

1. **Input Validation**

   - Required field checks
   - Format validation
   - Range verification

2. **Error States**
   - Missing data handling
   - API failure recovery
   - Storage error management

### Integration Points

1. **Views**

   - RecipeDetails.xaml
   - EditRecipe.xaml

2. **Services**
   - Recipe storage service
   - API service
   - Image handling service

### Best Practices

1. **Code Organization**

   - Separate concerns
   - Use MVVM pattern
   - Implement INotifyPropertyChanged

2. **Performance**

   - Lazy loading
   - Efficient data binding
   - Resource management

3. **Maintainability**
   - Clear documentation
   - Unit tests
   - Error logging

### Example Usage

```csharp
// Initialize ViewModel
var viewModel = new RecipeDetailsViewModel(recipe);

// Bind to View
recipeView.DataContext = viewModel;

// Save changes
await viewModel.SaveRecipeAsync();
```