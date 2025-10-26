using System.Diagnostics;
using Python.Runtime;
using Python.Included;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeApp.Controls.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WebBrowserPage : NavigatorPage
{
    [ObservableProperty] private partial string SearchBarText { get; set; } = "";
    [ObservableProperty] private partial string DownloadStatus { get; set; } = "";


    [ObservableProperty] private partial Visibility ProgressBarVis { get; set; } = Visibility.Collapsed;
    [ObservableProperty] private partial Visibility DownloadVis { get; set; } = Visibility.Visible;
    
    public WebBrowserPage(Navigator? nav = null) : base(nav)
    {
        this.InitializeComponent();

        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await WebViewControl.EnsureCoreWebView2Async();
            WebViewControl.CoreWebView2.Navigate("https://google.com");
            WebViewControl.CoreWebView2.SourceChanged += (_, _) 
                => { SearchBarText=WebViewControl.CoreWebView2.Source; };
            WebViewControl.CoreWebView2.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                WebViewControl.CoreWebView2.Navigate(args.Uri);
            };
        }
        catch (Exception ex)
        {
            // Handle initialization errors
        }
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoBack();
    }

    private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
    {
        WebViewControl.GoForward();

    }

    private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
    {
        if (Uri.TryCreate(SearchBarText, UriKind.Absolute, out var uri))
            WebViewControl.CoreWebView2.Navigate(SearchBarText);
        else
            WebViewControl.CoreWebView2.Navigate($"https://google.com/search?q={SearchBarText}");
    }
    
    
    private static bool _pythonInitialized = false;
    private async void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
    {
        DownloadVis = Visibility.Collapsed;
        ProgressBarVis = Visibility.Visible;
        
        DownloadStatus = "Downloading HTML...";

        var source = WebViewControl.Source;
        string html = await (WebViewControl.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML")?.AsTask() ?? new Task<string>(() => ""));
        
        // used Claude Sonnet 4.5 to generate a lot of the boilerplate for interacting with python using pythonnet (and python.included)
        // had to go through and really work to get his to work.
        try
        {
            var recipeInfo = await Task.Run(async () =>
            {
                if (!_pythonInitialized)
                {
                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsPythonInstalled())
                    {
                        Debug.WriteLine("Installing python...");
                    }

                    await Installer.SetupPython();

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsPipInstalled())
                    {
                        Debug.WriteLine("Installing pip...");
                        await Installer.TryInstallPip();
                    }

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsModuleInstalled("setuptools"))
                    {
                        Debug.WriteLine("Installing setuptools...");
                        await Installer.PipInstallModule("setuptools");
                    }

                    DownloadStatus = "Verifying python installation...";

                    if (!Installer.IsModuleInstalled("wheel"))
                    {
                        Debug.WriteLine("Installing wheel...");
                        await Installer.PipInstallModule("wheel");
                    }

                    DownloadStatus = "Verifying python installation...";

                    // This is way too complicated, why can't it be simple for just once.
                    // have to use the no build isolation flag for this to work due to the embedded python runtime
                    if (!Installer.IsModuleInstalled("recipe_scrapers"))
                    {
                        DownloadStatus = "Installing recipe scrapers...";

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(Installer.EmbeddedPythonHome, "python.exe"),
                                Arguments = "-m pip install --no-build-isolation recipe-scrapers",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                WorkingDirectory = Installer.EmbeddedPythonHome
                            }
                        };
                        
                        process.Start();
                        
                        var outputTask = process.StandardOutput.ReadToEndAsync();
                        var errorTask = process.StandardError.ReadToEndAsync();
    
                        await process.WaitForExitAsync();
    
                        string output = await outputTask;
                        string error = await errorTask;
    
                        Debug.WriteLine($"Exit Code: {process.ExitCode}");
                        Debug.WriteLine($"Output: {output}");
                        Debug.WriteLine($"Error: {error}");
                    }

                    PythonEngine.Initialize();
                    
                    _pythonInitialized = true;
                }
            
                DownloadStatus = "Extracting Recipe...";
            
                using (Py.GIL())
                {
                    dynamic scrapers = Py.Import("recipe_scrapers");
                    dynamic scraper = scrapers.scrape_html(html: html, org_url: source.ToString());

                    return new
                    {
                        Title = scraper.title().ToString(),
                        Ingredients = ((IEnumerable<dynamic>)scraper.ingredients())
                            .Select(i => i.ToString()).ToList(),
                        Instructions = scraper.instructions().ToString(),
                        TotalTime = scraper.total_time()?.ToString(),
                        Yields = scraper.yields()?.ToString()
                    };
                }
            });
            
            DownloadStatus = "Processing Recipe...";

        }
        catch (Exception exception)
        {
            DownloadStatus = $"Error: {exception.Message}";
            await Task.Delay(5000);
        }
        
        DownloadVis = Visibility.Visible;
        ProgressBarVis = Visibility.Collapsed;
    }
}
