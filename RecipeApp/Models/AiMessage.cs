using RecipeApp.Enums;

namespace RecipeApp.Models;


[ObservableObject]
public partial class AiMessage
{
    [ObservableProperty] public partial Sender Sender { get; set; }
    [ObservableProperty] public partial string Message { get; set; }
    
    public Thickness Margin => Sender switch
    {
        Sender.User => new Thickness(10, 10, 50, 10),
        Sender.Assistant => new Thickness(50, 10, 10, 10),
        _ => new Thickness(100, 10, 100, 10),
    };
    
    public AiMessage(Sender sender, string message)
    {
        Sender = sender;
        Message = message;
    }
}
