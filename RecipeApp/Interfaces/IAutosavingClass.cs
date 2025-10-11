namespace RecipeApp.Interfaces;

/// <summary>
/// Handles the saving and loading of a list of items automatically.
/// This includes adding/removing items and when any observable property on an observable object the list items change.
/// </summary>
/// <typeparam name="T">The type to create the list for (probably the same class that inherits from this class).</typeparam>
public abstract class IAutosavingClass<T> : ObservableObject where T : ObservableObject
{
    private static List<T>? Items { get; set; }
    private static string? SaveFilePath { get; set; }
    private static bool _inSave;
    private static bool _requireResave;
    
    /// <summary>
    /// Loading data if needed.
    /// </summary>
    /// <returns>a task containing a readonly collection of recipes</returns>
    public static async Task<IReadOnlyCollection<T>> GetAll()
    {
        if (Items is null) await LoadItems();
        return Items!;
    }
    
    /// <summary>
    /// Loads the list of items if needed.
    /// Adds an item to the list of items.
    /// Saves the list of items.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task Add(T item)
    {
        if (Items is null) await LoadItems();
        
        Items!.Add(item);
        
        item.PropertyChanged += ListObjectChanged;
        
        await SaveItems();
    }

    /// <summary>
    /// Loads the list of items if needed.
    /// Removes an item from the list of items.
    /// Saves the list of items.
    /// </summary>
    /// <param name="itemsToRemove"></param>
    public static async Task Remove(params T[] itemsToRemove)
    {
        if (Items is null) await LoadItems();

        foreach (var item in itemsToRemove)
        {
            Items!.Remove(item);
            item.PropertyChanged -= ListObjectChanged;
        }
        
        await SaveItems();
    }

    private static async Task LoadItems()
    {
        SaveFilePath ??= GetSaveFilePath();

        if (File.Exists(SaveFilePath) && await File.ReadAllTextAsync(SaveFilePath) is { Length: > 0 } fileData)
        {
            for (var i = 0; i < 3 && Items is null; i++) 
            {
                try
                {
                    Items = JsonSerializer.Deserialize<List<T>>(fileData) ?? throw new Exception();
                    
                    foreach (var item in Items)
                        item.PropertyChanged += ListObjectChanged;
                }
                catch (Exception)
                {
                    // ignored.
                }
            }

            Items ??= [];
        }
        else
            Items = [];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="force">Should only be true when calling from within the function
    /// to say that it should try to save no matter what</param>
    private static async Task SaveItems(bool force = false)
    {
        SaveFilePath ??= GetSaveFilePath();
        
        if (_inSave && !force)
        {
            _requireResave = true;
            return;
        }

        _inSave = true;

        if (!Directory.Exists(Path.GetDirectoryName(SaveFilePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!); 
        
        var json = JsonSerializer.Serialize(Items);
        await File.WriteAllTextAsync(SaveFilePath, json);
        
        _inSave = false;

        if (_requireResave)
        {
            _requireResave = false;
            await SaveItems(true);
        }    
    }

    private static string GetSaveFilePath()
    {
        var fileName = $"{typeof(T).Name}List.json";
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        return Path.Combine(appdata, "RecipeApp", fileName);
    }
    
    private static async void ListObjectChanged(object? sender, PropertyChangedEventArgs e)
    {
        await SaveItems();
    }
}
