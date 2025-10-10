# Introduction

## Application Architecture
TODO

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
