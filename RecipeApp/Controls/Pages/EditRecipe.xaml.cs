using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RecipeApp.Models;

namespace RecipeApp.Controls.Pages
{
    public sealed partial class EditRecipe : NavigatorPage
    {
        public SavedRecipe Recipe { get; }

        public EditRecipe(Navigator navigator, SavedRecipe recipe) : base(navigator)
        {
            Recipe = recipe;
            this.InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Since SavedRecipe inherits from IAutosavingClass, changes are automatically saved
            // We just need to show a confirmation and navigate back
            var infoBar = new InfoBar
            {
                Title = "Changes Saved",
                Message = "Your changes have been saved successfully.",
                Severity = InfoBarSeverity.Success,
                IsOpen = true,
                XamlRoot = this.XamlRoot
            };

            // Return to the recipe details page
            await Navigator.TryGoBack();
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Return to the recipe details page without saving changes
            await Navigator.TryGoBack();
        }
    }
}
