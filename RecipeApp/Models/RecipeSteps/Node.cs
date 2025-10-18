namespace RecipeApp.Models.RecipeSteps;

public record Node(string Title, List<IStep> Next)
{
    public Node(string title, IStep? next) : this(title, next is null ? [] : [ next ]) { }
};

