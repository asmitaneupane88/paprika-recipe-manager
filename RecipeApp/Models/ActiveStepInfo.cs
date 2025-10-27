using RecipeApp.Models.RecipeSteps;

namespace RecipeApp.Models;

public partial class ActiveStepInfo : IAutosavingClass<ActiveStepInfo>
{
    [ObservableProperty] public partial string RecipeTitle { get; set; }
    [ObservableProperty] public partial string RecipeImageUrl { get; set; }
    [ObservableProperty] public partial Guid RecipeId { get; set; }
    
    [ObservableProperty] public partial double TimeLeft { get; set; }
        
    public IStep CurrentStep { get; set { SetProperty( ref field, value); ResetBindableContent(); }}
    
    [ObservableProperty] public partial List<StepIngredientUsage> IngredientsUsed { get; set; }

    [JsonIgnore] public object BindableContent => GetBindableContent();
    [JsonIgnore] public Visibility NextButtonsVisible => CurrentStep is MergeStep ? Visibility.Collapsed : Visibility.Visible;

    private object GetBindableContent()
    {
        return CurrentStep switch
        {
            StartStep startStep => GetStartDescription(startStep),
            MergeStep mergeStep => "Waiting for other steps to be completed...",
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
                var maxString = (path.MaxIngredients
                    .FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.CurrentCultureIgnoreCase))
                    is { } maxIngredient && (maxIngredient.Quantity - 0.001) > ingredient.Quantity)
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
        return new StackPanel();
    }
    
    public void ResetBindableContent()
    {
        OnPropertyChanged(nameof(BindableContent));
        OnPropertyChanged(nameof(NextButtonsVisible));
    }
    
    public void AddIngredientsForStep(IStep step, List<RecipeIngredient> ingredients)
    {
        var existing = IngredientsUsed.FirstOrDefault(x => ReferenceEquals(x.Step, step));
        
        if (existing != null)
        {
            existing.Ingredients = ingredients;
        }
        else
        {
            IngredientsUsed.Add(new StepIngredientUsage
            {
                Step = step,
                Ingredients = ingredients
            });
        }
    }
    
    public List<RecipeIngredient>? GetIngredientsForStep(IStep step)
    {
        return IngredientsUsed.FirstOrDefault(x => ReferenceEquals(x.Step, step))?.Ingredients;
    }
}

public class StepIngredientUsage
{
    public IStep Step { get; set; } = null!;
    public List<RecipeIngredient> Ingredients { get; set; } = [];
}
