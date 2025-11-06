using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.Storage;
using Windows.Storage.Pickers;

using RecipeApp.Models;
using RecipeApp.Services;
using Uno.Extensions;

using System.Drawing.Printing;
using Windows.Devices.Midi;
using Windows.Graphics.Printing;

namespace RecipeApp.Controls.Pages;

public sealed partial class RecipeListPage : NavigatorPage
{
    public RecipeListPage()
    {
        this.InitializeComponent();
    }

private const int AllCategorySortOrder = -20252025;

    private readonly string PdfSavePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "SavedPdfs");
    
    [ObservableProperty] private partial ObservableCollection<RecipeCard> AllRecipes { get; set; } = [];
    [ObservableProperty] private partial ObservableCollection<RecipeCard> FilteredRecipes { get; set; } = [];
    
    private string SearchText
    {
        get;
        set { SetProperty(ref field, value); _ = UpdateShownRecipes(); }
    } = "";
    
    private SavedCategory SelectedCategory { get; set { SetProperty(ref field, value); _ = UpdateShownRecipes(); } }
    [ObservableProperty] private partial ObservableCollection<SavedCategory> Categories { get; set; } = [];
    [ObservableProperty] private partial bool CardsSelected { get; set; } = false;
    [ObservableProperty] private partial Visibility ListVisibility { get; set; } = Visibility.Visible;
    [ObservableProperty] private partial Visibility GridVisibility { get; set; } = Visibility.Collapsed;
    
    public RecipeListPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();
        
        InitializeRecipes()
            .ContinueWith(_ => UpdateShownCategories()
                .ContinueWith(_ => UpdateShownRecipes()));
        
    }

    private async Task InitializeRecipes()
    {
        var recipes = await SavedRecipe.GetAll();
        
        AllRecipes = recipes
            .Select(r => new RecipeCard { SavedRecipe = r, IsSelected = false })
            .ToObservableCollection();
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
        FilteredRecipes = AllRecipes
            .Where(r => r.SavedRecipe.Title.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(r => SelectedCategory.SortOrder == AllCategorySortOrder
                        || (r.SavedRecipe.Category is not null 
                            && r.SavedRecipe.Category.Trim()
                                .Equals(SelectedCategory.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            .ToObservableCollection();
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
        var recipesToRemove = GetSelectedRecipeCards();
        
        await SavedRecipe.Remove(recipesToRemove.Select(r => r.SavedRecipe).ToArray());
        recipesToRemove.ForEach(r => AllRecipes.Remove(r));
        
        await UpdateShownRecipes();
    }
    
    private async void OnButtonUploadPdfClick(object sender, RoutedEventArgs e)=>
        await UploadRecipePdfAsync();
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
            return;

        try
        {
            var localFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RecipeAppFiles");
            Directory.CreateDirectory(localFolderPath);

            var savedFilePath = Path.Combine(localFolderPath, file.Name);
            await file.CopyAsync(await StorageFolder.GetFolderFromPathAsync(localFolderPath), file.Name,
                NameCollisionOption.GenerateUniqueName);

            var htmlText = await FileHelper.ConvertPdfToTextAsync(savedFilePath);
            var htmlPath = Path.Combine(localFolderPath, $"{Path.GetFileNameWithoutExtension(savedFilePath)}.html");
            await File.WriteAllTextAsync(htmlPath, htmlText);
            
            var existing = AllRecipes.FirstOrDefault(r =>
                string.Equals(r.SavedRecipe.PdfPath, savedFilePath, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                await new ContentDialog
                {
                    Title = "Duplicate PDF Detected",
                    Content = $"A recipe for {Path.GetFileName(savedFilePath)} already exists: {existing.SavedRecipe.Title}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
                return;
            }

            var selectedRecipe = GetSelectedRecipes().FirstOrDefault();
            if (selectedRecipe != null)
            {
                selectedRecipe.PdfPath = savedFilePath;
                selectedRecipe.HtmlPath = htmlPath;
                await SavedRecipe.Update(selectedRecipe);
            }

            try
            {
                var aiRecipe = await AiHelper.StringToSavedRecipe(htmlText);
                if (aiRecipe != null)
                {
                    aiRecipe.PdfPath = savedFilePath;
                    aiRecipe.HtmlPath = htmlPath; 
                    await SavedRecipe.Add(aiRecipe);
                    AllRecipes.Add(new RecipeCard { SavedRecipe = aiRecipe, IsSelected = false });
                }
                    

                var aiDialog = new ContentDialog
                {
                    Title = "AI Recipe Created",
                    Content = aiRecipe != null? $"Created new recipe from PDF:\n{aiRecipe.Title}": "AI returned no recipe",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await aiDialog.ShowAsync();
            }
            catch (Exception aiEx)
            {
                await new ContentDialog
                {
                    Title = "AI Conversion Failed",
                    Content = $"Could not generate recipe automatically.\n{aiEx.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }

            await new ContentDialog
            {
                Title = "PDF Upload & Converted",
                Content = $"File saved:\n{savedFilePath}\n\nHTML created:\n{htmlPath}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
            await UpdateShownRecipes();
        }
        catch (Exception ex)
        {
            await new ContentDialog
            {
                Title = "PDF Upload Failed",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            }.ShowAsync();
        }
    }

    private async void OnPdfIconClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: RecipeCard card } && !string.IsNullOrEmpty(card.SavedRecipe.PdfPath))
        {
            var pdfPath = card.SavedRecipe.PdfPath;

            if (File.Exists(pdfPath))
            {
#if WINDOWS
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(pdfPath);
            await Windows.System.Launcher.LaunchFileAsync(file);
#else
                // For Uno on other platforms (if any)
                var uri = new Uri(pdfPath);
                await Windows.System.Launcher.LaunchUriAsync(uri);
#endif
            }
            else
            {
                await new ContentDialog
                {
                    Title = "PDF Not Found",
                    Content = $"The file could not be found:\n{pdfPath}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
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

