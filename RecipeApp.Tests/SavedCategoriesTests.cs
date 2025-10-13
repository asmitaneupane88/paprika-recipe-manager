using RecipeApp.Models;

namespace RecipeApp.Tests;

public class SavedCategoriesTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task AddCategoriesTest()
    {
        List<SavedCategory> categoriesToAdd =
        [
            new()
            {
                Name = "Lunch",
            },
            new()
            {
                Name = "Dinner"
            }
        ];

        if ((await SavedCategory.GetAll()).Count == 0)
        {
            foreach (var category in categoriesToAdd)
                await SavedCategory.Add(category);
        }
        
        Assert.Pass();
    }

    [Test]
    public async Task AssignCategories()
    {
        var recipes = (await SavedRecipe.GetAll()).ToArray();
        var categories = (await SavedCategory.GetAll()).ToArray();
        
        if (categories.Length < 2) return;
        
        for (var i = 0; i < recipes.Length; i++)
        {
            recipes[i].Category = i % 2 == 0 ? categories[0].Name : categories[1].Name;
        }
    }
}
