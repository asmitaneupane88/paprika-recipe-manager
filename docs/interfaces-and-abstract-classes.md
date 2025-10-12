# Interfaces and Abstract Classes
Note that this is not a complete list of interfaces and abstract classes.
Only the most important interfaces and abstract classes are documented here (view the API docs for the full list).

## IAutosavingClass
This class handles the saving and loading of items to and from a json file with the file path "%APPDATA%/RecipeApp/{nameof(T)}List.json"
(this is the file path on Windows, other platforms use the equivalent of application data).

This class contains static Add, Remove, and GetAll methods which should be used like the class itself is a list of objects.

See @RecipeApp.Interfaces.IAutosavingClass`1 for more information.
