using RecipeApp.Interfaces;
using RecipeApp.Services;

namespace RecipeApp.Tests
{
    [TestFixture]
    public class ApiUnitTests
    {
        private IRecipeService _recipeService = null!;
    
        [SetUp]
	    public void Setup()
        {
            _recipeService = new ApiControl();
	    }
    
        [Test]
        public async Task Api_ShouldReturnRecipes_ForValidQuery()
        {
            string query = "Chicken";
            var recipes = await _recipeService.SearchAsync(query);
        
            Assert.That(recipes, Is.Not.Null);
            Assert.That(recipes.Count, Is.GreaterThan(0));
        
            Console.WriteLine($"Found {recipes.Count} recipes for  '{query}':");
            foreach (var recipe in  recipes)
            {
                Console.WriteLine($" - {recipe.Name} ({recipe.Ingredients})");
            }
        }

        [Test]
        public async Task Api_ShouldHandleInvalidQuery()
        {
            string query = "!!nonexistent!!";
            var recipes = await _recipeService.SearchAsync(query);
        
            Assert.That(recipes, Is.Not.Null, "API return null.");
            Assert.That(recipes.Count, Is.GreaterThanOrEqualTo(0), "Should not throw exception for invalid queries.");
        }

        [Test]
        public void Api_ShouldHandleEmptyQuery_WithoutException()
        {
            var api = new ApiControl();
        
            Assert.DoesNotThrowAsync(async () => await api.SearchAsync(string.Empty));
        }
    }
}
