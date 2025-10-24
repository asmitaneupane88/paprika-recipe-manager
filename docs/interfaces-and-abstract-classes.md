# Interfaces and Abstract Classes
Note that this is not a complete list of interfaces and abstract classes.
Only the most important interfaces and abstract classes are documented here (view the API docs for the full list).

## IAutosavingClass
This class handles the saving and loading of items to and from a json file with the file path "%APPDATA%/RecipeApp/{nameof(T)}List.json"
(this is the file path on Windows, other platforms use the equivalent of application data).

This class contains static Add, Remove, and GetAll methods which should be used like the class itself is a list of objects.

See @RecipeApp.Interfaces.IAutosavingClass`1 for more information.

## IStep
This class is used by the [RecipeSteps](classes.md#step-classes) located in this the linked section of the classes.

This class implements some shared properties along with containing information at the top of the file to help the JSON
serializer know how to deserialize into the correct step type. This abstract class also implements a search function
called `GetNestedPathInfo` to find information about the max/min time and ingrediens of all steps that the current step can lead to.

See @RecipeApp.Interfaces.IStep for more information.

## IStepControl

This abstract class is the foundation for each of the step nodes used in the editor with shared properties, events, and functions.

see @RecipeApp.Interfaces.IStepControl for more information.
