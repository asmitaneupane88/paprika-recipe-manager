# Welcome to Paprika Recipe Manager

This is the documentation for the Paprika Recipe Manager project.

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or VS Code
3. Build and run the project

## Project Structure

- `RecipeApp/` - Main application project
  - `Models/` - Data models
  - `ViewModels/` - MVVM view models
  - `Controls/` - UI controls and pages
  - `Services/` - Business logic and services

## Features

- Search recipes from MealDB
- Save favorite recipes
- View detailed recipe information
- Manage your recipe collection

### Supported Platforms
- Linux (needs testing)
- Windows 10 (needs testing)
- Windows 11
- MacOS

## Application Ui/Ux
TODO

### Shell
The app uses the @RecipeApp.Controls.Shell class as the main container.
This container is responsible for the navigation menu and the header.
The content of the shell is determined by the current page of the @RecipeApp.Models.Navigator
which handles all navigation and keeping track of the current page along with history.
