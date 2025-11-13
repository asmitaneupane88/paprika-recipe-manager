using RecipeApp.Enums;

namespace RecipeApp.Models;


[ObservableObject]
public partial class RecipeMessege
{
    [ObservableProperty] public partial Sender Sender { get; set; }
    [ObservableProperty] public partial string Message { get; set; }
    
    public Thickness Margin => Sender switch
    {
        Sender.User => new Thickness(0, 0, 10, 0),
        Sender.Assistant => new Thickness(10, 0, 0, 0),
        _ => new Thickness(20, 0, 20, 0),
    };
    
    public RecipeMessege(Sender sender, string message)
    {
        Sender = sender;
        Message = message;
    }
}
