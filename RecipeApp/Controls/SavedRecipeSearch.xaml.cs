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
    private static readonly SavedTag AddTag = new() { Name = "Add Tag" };
    
    [ObservableProperty] private partial ObservableCollection<RecipeCard> AllRecipes { get; set; } = [];

    [ObservableProperty] private partial ObservableCollection<RecipeCard> FilteredRecipes { get; set; } = [];
    
    [ObservableProperty] public partial ObservableCollection<RecipeCard> SelectedRecipes { get; set; } = [];

    [ObservableProperty] private partial ObservableCollection<SavedTag> SelectedTags { get; set; } = [];
    
    private string SearchText
    {
        get;
        set { SetProperty(ref field, value); UpdateShownRecipes(); }
    } = "";
    
    private void OnTagComboSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: SavedTag tag } comboBox && comboBox.SelectedIndex != 0)
        {
            SelectedTags.Add(tag);
            UpdateShownTags();
            UpdateShownRecipes();
        }
    }
    
    [ObservableProperty] private partial ObservableCollection<SavedTag> FilteredTags { get; set; } = [];
    [ObservableProperty] private partial ObservableCollection<SavedTag> AllTags { get; set; } = [];
    
    public SavedRecipeSearch()
    {
        this.InitializeComponent();
        
       InitializeRecipes()
            .ContinueWith(_ => UpdateAllTags()
            .ContinueWith(_ => UpdateShownRecipes()));
        
    }

    private async Task InitializeRecipes()
    {
        var recipes = await SavedRecipe.GetAll();
        
        AllRecipes = recipes
            .Select(r => new RecipeCard { SavedRecipe = r, IsSelected = false })
            .ToObservableCollection();
    }

    private async Task UpdateAllTags()
    {
        AllTags = (await SavedTag.GetAll())
            .Prepend(AddTag)
            .ToObservableCollection();
        UpdateShownTags();
    }

    private void UpdateShownTags()
    {
        FilteredTags = AllTags
            .Where(t => !SelectedTags.Contains(t))
            .ToObservableCollection();
        
        TagCombo.SelectedIndex = 0; // keep it on the prepended add tag
    }
    
    private void UpdateShownRecipes()
    {
        FilteredRecipes = AllRecipes
            .Where(r => r.SavedRecipe.Title.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .Where(r => SelectedTags.All(tag => r.SavedRecipe.Tags
                .Any(recipeTag => recipeTag.Trim()
                    .Equals(tag.Name.Trim(), StringComparison.CurrentCultureIgnoreCase))))
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
    
    private void TagCard_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement { DataContext: SavedTag tag })
        {
            SelectedTags.Remove(tag);
            UpdateShownRecipes();
            UpdateShownTags();
        }
    }
}

