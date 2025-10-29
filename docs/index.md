# Welcome to PseudoChef

This is the documentation for the PseudoChef. This application is based off of the app Paprika. Furthermore, the app is using Uno Platform and .NET 9.

## Features

- Search recipes from MealDB (API)
- Save favorite recipes
- Recipe Step Viewer
- Meal Planner
- Web Browser

## Project Structure

- `RecipeApp/` - Main application project
  - `Interfaces/` - Interfaces and abstract classes
   for all interaction recipe step controls; handles mouse/node events and visual interaction with `InNode` / `OutNode` connections in the recipe step editor.
  - `Models/` - Data models
    - `RecipeSteps`/ -Contains all models representing individual recipe steps and their connection nodes, forming the logical and visual backbone of the recipe workflow editor.
  - `ViewModels/` - Contains the MVVM view models that manage the state, logic, and commands for recipe-related pages such as search and recipe details.

  - `Controls/` - UI controls and pages
    - `Pages/` - Application pages
    - `StepControls/` - XAML-based user controls that visually represent recipe step widgets and connection lines in the flow editor.
    - `Services/` - Business logic and services
  - `Converters`/ - Value converters used in data binding to transform model data for UI display, such as visibility toggles and time formatting.
  - `Enums`/ - Defines unit categories
