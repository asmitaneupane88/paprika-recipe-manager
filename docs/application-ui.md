# Application UI
The framework used is Uno Platform, which supports almost any platform.
The window holds the @RecipeApp.Controls.Shell which acts as the main container.

## Shell
This container is responsible for the navigation menu and the header.
The content of the shell is determined by the current page of the @RecipeApp.Models.Navigator
which handles all navigation and keeping track of the current page along with history.

## Pages
The following is an overview of the pages in the application.

### Saved Recipes
The saved recipes page is a list of all saved recipes along with the ability to search for a specific recipe and filter by recipe category.
This page also allows for multi-selecting recipes to either delete them or save them as a PDF.

See @RecipeApp.Controls.Pages.RecipePage for more details.

### Recipe Details
TODO



### MealDB Recipe Lookup
TODO

see @RecipeApp.Controls.Pages.SearchPage for more details.
