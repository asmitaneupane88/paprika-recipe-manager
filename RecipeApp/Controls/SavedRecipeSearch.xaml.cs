using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RecipeApp.Controls;

[ObservableObject]
public sealed partial class SavedRecipeSearch : UserControl
{
    private const int AllCategorySortOrder = -20252025;
    
    [ObservableProperty] private partial ObservableCollection<RecipeCard> AllRecipes { get; set; } = [];

    [ObservableProperty] private partial ObservableCollection<RecipeCard> FilteredRecipes { get; set; } = [];
    
    [ObservableProperty] public partial ObservableCollection<RecipeCard> SelectedRecipes { get; set; } = [];

    
    private string SearchText
    {
        get;
        set { SetProperty(ref field, value); _ = UpdateShownRecipes(); }
    } = "";
    
    private SavedCategory SelectedCategory { get; set { SetProperty(ref field, value); _ = UpdateShownRecipes(); } }
    [ObservableProperty] private partial ObservableCollection<SavedCategory> Categories { get; set; } = [];
    
    public SavedRecipeSearch()
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

    private RecipeCard[] GetSelectedRecipeCards()
    {
        return AllRecipes
            .Where(c => c.IsSelected)
            .ToArray();
    }
    
    private void OnRecipeCardChecked(object sender, RoutedEventArgs e)
    {
        RefreshSelected();
    }
    private void RefreshSelected()
    {
        SelectedRecipes = GetSelectedRecipeCards().ToObservableCollection();
    }
}

