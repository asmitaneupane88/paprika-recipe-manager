using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public partial class SearchViewModel: ObservableObject
    {
        private readonly IRecipeService _recipeService;
        
        [ObservableProperty] private string _query = string.Empty;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private bool isEmpty;

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public IAsyncRelayCommand SearchCommand { get; }

        public SearchViewModel(IRecipeService recipeService)
        {
            _recipeService = recipeService;
            SearchCommand = new AsyncRelayCommand(SearchAsync, CanSearch);
        }
        private bool CanSearch() => !string.IsNullOrWhiteSpace(Query);

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                Recipes.Clear();

                var results = await _recipeService.SearchAsync(Query);
                foreach (var r in results)
                    Recipes.Add(r);

                IsEmpty = Recipes.Count == 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        
    }
}
