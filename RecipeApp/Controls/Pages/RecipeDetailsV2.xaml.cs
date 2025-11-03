using Microsoft.UI.Xaml.Input;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeDetailsV2 : NavigatorPage
{
    [ObservableProperty] public partial SavedRecipe SavedRecipe { get; set; }
    [ObservableProperty] public partial Visibility EditImageHover { get; set; } = Visibility.Collapsed;
    
    public RecipeDetailsV2(Navigator? nav = null, SavedRecipe? savedRecipe = null) : base(nav)
    {
        InitializeComponent();

        if (savedRecipe is null)
            _ = Navigator.TryGoBack();
        
        SavedRecipe = savedRecipe!;
        
    }

    private async void ImageGrid_OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var textbox = new TextBox
        {
            PlaceholderText = "Recipe Image Url",
            Text = SavedRecipe.ImageUrl
        };

        var pickButton = new Button
        {
            Content = new SymbolIcon(Symbol.Folder)
        };

        pickButton.Click += async (_, _) =>
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail,
                FileTypeFilter = { ".jpg", ".jpeg", ".png" }
            };

            var file = await picker.PickSingleFileAsync();

            if (file is not null)
                textbox.Text = file.Path;
        };

        var stack = new StackPanel
        {
            Children =
            {
                textbox,
                pickButton
            }
        };
        
        var popup = new ContentDialog()
        {
            Content = stack,
            Title = "Change Recipe Image Url",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            XamlRoot = this.XamlRoot
        };
        
        textbox.TextChanged += (_, _) =>
        {
            popup.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(textbox.Text);
        };
        
        if (await popup.ShowAsync() == ContentDialogResult.Primary)
        {
            SavedRecipe.ImageUrl = textbox.Text;
        }
    }

    private void ImageGrid_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        EditImageHover = Visibility.Collapsed;
    }

    private void ImageGrid_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        EditImageHover = Visibility.Visible;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (SavedRecipe.AdvancedSteps || true /*TODO: temp*/)
        {
            Navigator.Navigate(new StepEditor(SavedRecipe), $"Advance Step Editor: {SavedRecipe.Title}");
        }
        else
        {
            //TODO: show the simple steps editor
        }
    }

    public override async Task Restore()
    {
        await Models.SavedRecipe.SaveAll();
    }
}

