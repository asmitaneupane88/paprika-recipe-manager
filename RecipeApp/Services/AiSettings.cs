using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
    
namespace RecipeApp.Services;

public partial class AiSettings : ObservableObject
{
    [ObservableProperty] public partial string ApiKey { get; set; }
    [ObservableProperty] public partial string Endpoint { get; set; }
    [ObservableProperty] public partial string LastUsedModel { get; set; }

    private static string? _saveFilePath;
    private static AiSettings? _settings;
    private static bool _inSave;
    private static bool _requireResave;

    public AiSettings()
    {
        PropertyChanged += (_, _) => _ = SaveSettings();
    }
    
    public static async Task<AiSettings> GetSettings()
    {
        if (_settings is not null) return _settings;

        _saveFilePath ??= GetSaveFilePath();
        Console.WriteLine($"[DEBUG] Loading AI settings from: {_saveFilePath}");
        
        if (File.Exists(_saveFilePath) && await File.ReadAllTextAsync(_saveFilePath) is { Length: > 0 } fileData)
        {
            for (var i = 0; i < 3 && _settings is null; i++) 
            {
                try
                {
                    _settings = JsonSerializer.Deserialize<AiSettings>(fileData) ?? throw new Exception();
                    Console.WriteLine($"[DEBUG] Endpoint loaded: {_settings.Endpoint}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to load AiSettings.json: {ex.Message}");
                    _settings = new AiSettings();
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
        
        var dir = Path.GetDirectoryName(_saveFilePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!); 
        
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions {WriteIndented = true});
        await File.WriteAllTextAsync(_saveFilePath, json);
        Console.WriteLine($"[DEBUG] Saved AI settings to {_saveFilePath}");
        
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
        var LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        return Path.Combine(LocalAppData, "O=RecipeApp", "com.companyname.RecipeApp", "Settings", fileName);
    }
}
