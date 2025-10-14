# Welcome to PseudoChef

This is the documentation for the PseudoChef. This application is based off of the app Paprika. Furthermore, the app is using Uno Platform and .NET 9.

## Features

- Search recipes from MealDB (API)
- Save favorite recipes
- View detailed recipe information
- Edit recipe details
- Manage your recipe collection

## Project Structure

- `RecipeApp/` - Main application project
  - `Interfaces/` - Interfaces and abstract classes
    - `IRecipe.cs` - Base recipe interface
    - `IRecipeService.cs` - Recipe service interface
    - `IAutosavingClass.cs` - Autosaving functionality interface
  - `Models/` - Data models
    - `Recipe.cs` - Core recipe model with all recipe properties
    - `SavedRecipe.cs` - Local storage recipe model
    - `RecipeIngredient.cs` - Ingredient data structure
    - `MealDbRecipe.cs` - API recipe model
    - `Navigator.cs` - Navigation state management
    - `Route.cs` - Route definitions
    - `RecipeSteps/` - Recipe step implementations
      - `TextStep.cs` - Text-based recipe step
      - `TimerStep.cs` - Timer-based recipe step
  - `ViewModels/` - MVVM view models
    - `RecipeDetailsViewModel.cs` - Recipe details logic
    - `SearchViewModel.cs` - Recipe search functionality
  - `Controls/` - UI controls and pages
    - `Pages/` - Application pages
      - `RecipeDetails.xaml` - Recipe details view
      - `EditRecipe.xaml` - Recipe editing interface
      - `SearchPage.xaml` - Recipe search interface
    - `Shell.xaml` - Main application shell
  - `Services/` - Business logic and services
    - `ApiControl.cs` - MealDB API integration
    - `RecipeConverter.cs` - Recipe format conversion
    - `FileHelper.cs` - File operations
