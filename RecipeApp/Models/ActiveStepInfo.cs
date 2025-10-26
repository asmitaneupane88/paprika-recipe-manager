using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

public partial class ActiveStepInfo : IAutosavingClass<ActiveStepInfo>
{
    [ObservableProperty] public partial string RecipeTitle { get; set; }
    [ObservableProperty] public partial string RecipeImageUrl { get; set; }
    [ObservableProperty] public partial Guid RecipeId { get; set; }
    
    [ObservableProperty] public partial double TimeLeft { get; set; }
        
    [ObservableProperty] public partial IStep CurrentStep { get; set; }
    
    [ObservableProperty] public partial Dictionary<IStep, RecipeIngredient> IngredientsUsed { get; set; }

    [JsonIgnore] public object BindableContent => GetBindableContent();

    private object GetBindableContent()
    {
        return CurrentStep switch
        {
            StartStep startStep => GetStartDescription(startStep),
            TextStep textStep => textStep.BindableDescription??"",
            TimerStep timerStep => GetTimerContent(timerStep),
            _ => "",
        };
    }
    
    private string GetStartDescription(StartStep startStep)
    {
        var paths = startStep.GetNestedPathInfo();

        var sb = new StringBuilder();

        foreach (var path in paths)
        {
            sb.AppendLine(path.OutNode.Title);
            sb.AppendLine($"    Cook time: {path.MinTotalTime}-{path.MaxTotalTime} minutes");
            sb.AppendLine($"    Ingredients: ");
            foreach (var ingredient in path.MinIngredients.OrderBy(i => i.Name))
            {
                var maxString = path.MaxIngredients
                    .FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase))
                    is { } maxIngredient && maxIngredient.Quantity > ingredient.Quantity
                    ? $"-{maxIngredient.Quantity}" 
                    : "";
                    
                sb.AppendLine($"        {ingredient.Name} ({ingredient.Quantity}{maxString} {ingredient.Unit})");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private StackPanel GetTimerContent(TimerStep timerStep)
    {
        throw new NotImplementedException();
    }
}
