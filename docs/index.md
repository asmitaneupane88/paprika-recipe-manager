# Welcome to Paprika Recipe Manager

This is the documentation for the Paprika Recipe Manager project.

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio or VS Code
3. Build and run the project

## Project Structure

- `RecipeApp/` - Main application project
  - `Intefaces/` - Interfaces and abstract classes
  - `Models/` - Data models
  - `ViewModels/` - MVVM view models
  - `Controls/` - UI controls and pages
  - `Services/` - Business logic and services

### Recommended IDE
- JetBrains Rider (easiest to work with in my opinion)

### Supported IDEs
- Any IDE that supports .NET 9 and the Uno Platform Extension
    - JetBrains Rider
    - Visual Studio
    - Visual Studio Code
    - Any others mentioned on the Uno Platform website

### Installation
1. It is recommended to first install from Uno Platform the UnoCheck command line tool to make sure you can build without issues.
2. It is recommended to email about getting a free student license for Uno Platform (needed for hot reloading and the UI designer).
3. Install a supported IDE and the Uno Platform Extension.
4. Clone the repository from GitHub and open the solution.
5. Build and verify that it runs without errors.


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
