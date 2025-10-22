using System.Drawing.Printing;
using System.Text;
using Windows.Graphics.Printing;
using Microsoft.UI.Xaml.Input;
using RecipeApp.Services;
using Uno.Extensions;
using Windows.Storage.Pickers;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using Windows.Devices.Midi;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeListPage : NavigatorPage
{
    public RecipeListPage()
    {
        this.InitializeComponent();
    }

private const int AllCategorySortOrder = -20252025;

    private readonly string PdfSavePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "SavedPdfs");
    
    private ObservableCollection<RecipeCard> FilteredRecipes { get; set => SetField(ref field, value); } = [];
    
    private string SearchText
    {
        get;
        set { SetField(ref field, value); _ = UpdateShownRecipes(); }
    } = "";
    
    private SavedCategory SelectedCategory { get; set { SetField(ref field, value); _ = UpdateShownRecipes(); } }
    private ObservableCollection<SavedCategory> Categories { get; set => SetField(ref field, value); } = [];
    
    private bool CardsSelected { get; set => SetField(ref field, value); } = false;
    private Visibility ListVisibility { get; set => SetField(ref field, value); } = Visibility.Visible;
    private Visibility GridVisibility { get; set => SetField(ref field, value); } = Visibility.Collapsed;
    
    public RecipeListPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        UpdateShownCategories()
            .ContinueWith(_ => UpdateShownRecipes());
        
    }

    private async Task UpdateShownCategories()
    {
        Categories = (await SavedCategory.GetAll()).ToObservableCollection();
        
        var allCategory = new SavedCategory
        {
            Name = "All Categories",
            SortOrder = AllCategorySortOrder
        };
        
        Categories.Insert(0, allCategory);
        SelectedCategory = allCategory;
    }
    
    private async Task UpdateShownRecipes()
    {
        var recipes = await SavedRecipe.GetAll();

        FilteredRecipes = recipes
            .Where(r => r.Title.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(r => SelectedCategory.SortOrder == AllCategorySortOrder
                        || (r.Category is not null 
                        && r.Category.Trim()
                            .Equals(SelectedCategory.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            .Select(r => new RecipeCard { SavedRecipe = r, IsSelected = false })
            .ToObservableCollection();
        
        RefreshSelected();
    }
    
    private void RefreshSelected()
    {
        CardsSelected = GetSelectedRecipeCards()
            .Any(c => c.IsSelected);
    }

    private RecipeCard[] GetSelectedRecipeCards()
    {
        return FilteredRecipes
            .Where(c => c.IsSelected)
            .ToArray();
    }

    private SavedRecipe[] GetSelectedRecipes()
    {
        return GetSelectedRecipeCards()
            .Select(c => c.SavedRecipe)
            .ToArray();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }

    private void OnRecipeCardClicked(object sender, RoutedEventArgs e)
    {
        if (sender is ListView { SelectedItem: RecipeCard rc } lv)
        {
            lv.SelectedItem = null;
            var details = new RecipeDetailsPage(Navigator, savedRecipe: rc.SavedRecipe);
            Navigator.Navigate(details, $"Recipe: {rc.SavedRecipe.Title}");
        }
    }

    private async void OnButtonAddClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Create New Recipe",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            XamlRoot = this.XamlRoot
        };
        
        var textbox = new TextBox
        {
            PlaceholderText="Recipe Title",
        };
        
        textbox.TextChanged += (s, _) =>
        {
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
        };
        
        dialog.Content = textbox;
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var newRecipe = new SavedRecipe()
            {
                Title = textbox.Text,
            };

            await SavedRecipe.Add(newRecipe);

            Navigator.Navigate(new EditRecipe(Navigator, newRecipe), title:$"Edit {newRecipe.Title}");
        }
    }

    private async void OnButtonRemoveClick(object sender, RoutedEventArgs e)
    {
        var recipesToRemove = GetSelectedRecipes();
        
        await SavedRecipe.Remove(recipesToRemove);
        
        await UpdateShownRecipes();
    }
    
    private async void OnButtonUploadPdfClick(object sender, RoutedEventArgs e)
    {
        await UploadRecipePdfAsync();
    }

    private async Task UploadRecipePdfAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".pdf");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

#if WINDOWS
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.Instance));
        
#endif
        var file = await picker.PickSingleFileAsync();

        if (file == null)
        {
            var cancelDialog = new ContentDialog
            {
                Title = "Upload Canceled",
                Content = "No file was selected.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await cancelDialog.ShowAsync();
            return;
        }

        try
        {
            Directory.CreateDirectory(PdfSavePath);
            
            var destinationPath = Path.Combine(PdfSavePath, file.Name);
            await file.CopyAsync(await StorageFolder.GetFolderFromPathAsync(PdfSavePath), file.Name, NameCollisionOption.GenerateUniqueName);
            
            //Attached to first selected recipe
            var selectedRecipe = GetSelectedRecipes().FirstOrDefault();
            if (selectedRecipe != null)
            {
                selectedRecipe.PdfPath =  destinationPath;
                await SavedRecipe.Update(selectedRecipe);  // assumes async persistence API
            }

            var dialog = new ContentDialog
            {
                Title = "PDF Uploaded Successfully",
                Content = $"Saved to: {destinationPath}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();

            await UpdateShownRecipes();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Upload Failed",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }    
    }

    private void OnButtonGroceryListClick(object sender, RoutedEventArgs e)
    {
        //TODO: create list from selected and open it
    }

    private async void OnButtonPrintClick(object sender, RoutedEventArgs e)
    {
        var recipesToPrint = GetSelectedRecipes();

        var sb = new StringBuilder();

        sb.AppendLine(SavedRecipe.HtmlHeader);
        
        foreach (var recipe in recipesToPrint)
        {
            sb.AppendLine(recipe.ConvertToHtml());
            sb.AppendLine("<hr/>");
        }

        sb.AppendLine(SavedRecipe.HtmlFooter);

        var html = sb.ToString();
        
        //TODO: look into moving from save file to print maybe?
        // I could not find a cross platform library to use and do not want to do each platform and have to test each one.

        await FileHelper.SaveHtmlAsPdf(html);
    }

    /// <inheritdoc />
    public override async Task Restore()
    {
        await UpdateShownRecipes();
        await base.Restore();
    }

    
}

