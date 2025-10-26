namespace RecipeApp.Models;

public partial class AiSettings : ObservableObject
{
    [ObservableProperty] public partial string ApiKey { get; set; }
    [ObservableProperty] public partial string Endpoint { get; set; }
    [ObservableProperty] public partial string LastUsedModel { get; set; }

    public AiSettings()
    {
        PropertyChanged += (_, _) => _ = SaveSettings();
    }
    
    private static string? _saveFilePath { get; set; }
    private static AiSettings? _settings;
    private static bool _inSave;
    private static bool _requireResave;
    public static async Task<AiSettings> GetSettings()
    {
        if (_settings is not null) return _settings;

        _saveFilePath ??= GetSaveFilePath();
        
        if (File.Exists(_saveFilePath) && await File.ReadAllTextAsync(_saveFilePath) is { Length: > 0 } fileData)
        {
            for (var i = 0; i < 3 && _settings is null; i++) 
            {
                try
                {
                    _settings = JsonSerializer.Deserialize<AiSettings>(fileData) ?? throw new Exception();
                }
                catch (Exception)
                {
                    // ignored.
                }
            }

            _settings ??= new AiSettings();
        }
        else
            _settings = new AiSettings();

        return _settings;
    }
    
    private static async Task SaveSettings(bool force = false)
    {
        _saveFilePath ??= GetSaveFilePath();
        
        if (_settings is null) return;
        
        if (_inSave && !force)
        {
            _requireResave = true;
            return;
        }

        _inSave = true;

        if (!Directory.Exists(Path.GetDirectoryName(_saveFilePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath)!); 
        
        var json = JsonSerializer.Serialize(_settings);
        await File.WriteAllTextAsync(_saveFilePath, json);
        
        _inSave = false;

        if (_requireResave)
        {
            _requireResave = false;
            await SaveSettings(true);
        }    
    }
    
    private static string GetSaveFilePath()
    {
        const string fileName = "AiSettings.json";
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        return Path.Combine(appdata, "RecipeApp", fileName);
    }
}
