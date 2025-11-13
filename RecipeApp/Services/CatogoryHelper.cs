namespace RecipeApp.Services
{
    public static class CategoryHelper
    {
        public static string AutoDetectCategory(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Uncategorized";

            string lower = name.ToLower();

            if (lower.Contains("milk") || lower.Contains("cheese") || lower.Contains("cream") || lower.Contains("yogurt")|| lower.Contains("butter"))
                return "Dairy";

            if (lower.Contains("beef") || lower.Contains("pork") || lower.Contains("chicken") || lower.Contains("meat"))
                return "Meat";

            if (lower.Contains("fish") || lower.Contains("salmon") || lower.Contains("prawn") || lower.Contains("shrimp"))
                return "Seafood";

            if (lower.Contains("apple") || lower.Contains("banana") || lower.Contains("orange") || lower.Contains("lemon"))
                return "Fruits";

            if (lower.Contains("onion") || lower.Contains("garlic") || lower.Contains("carrot") || lower.Contains("broccoli") ||
                lower.Contains("asparagus") || lower.Contains("vegetable"))
                return "Vegetables";

            if (lower.Contains("rice") || lower.Contains("flour") || lower.Contains("sugar"))
                return "Baking";

            if (lower.Contains("wine") || lower.Contains("juice") || lower.Contains("water"))
                return "Beverages";

            return "Others";
        }
    }
}
