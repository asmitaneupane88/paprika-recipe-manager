using Microsoft.UI.Xaml.Data;

namespace RecipeApp.Converters;

public class MinutesToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int minutes)
        {
            if (minutes < 60)
            {
                return $"{minutes} min";
            }
            else
            {
                int hours = minutes / 60;
                int remainingMinutes = minutes % 60;
                if (remainingMinutes == 0)
                {
                    return $"{hours} hr";
                }
                return $"{hours} hr {remainingMinutes} min";
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}