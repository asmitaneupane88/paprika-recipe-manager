# NuGet Packages
Only non-standard packages are listed here.
Uno platform bundles a lot of packages including SkiaSharp,
NUnit for unit tests, WinUi packages, and a lot of runtime related packages.

## Python.Included - 3.11.4 (must be this version)
This is used to bundle a embedded python runtime with the app.

**Do not update this package!**\
The reason for this is due to the newer versions of this package not working with Uno platform or something
(I have no clue exactly what the issue is), but it does not allow anything to install including PIP and the recipe-scrapers module.

### Python module: recipe-scrappers
Used to scrape data from websites before passing it to the Ai model.
This prevents having too much context by not passing the entire HTML of a site to the model.

### Pythonnet - (3.0.1)
Installed with Python.Included.
Used to call the recipe-scrapers module from C# code.

## PuppeteerSharp - Latest
Used to create a browser with no UI for use in converting html into a PDF for example.

## Microsoft.Web.WebView2 - Latest
Used to create a browser with a UI for use in the app (rendering a PDF or as a full on web browser).

## NUnit - Latest
Used for unit tests within the app

### FluentAssertions - Latest
Used for unit tests within the app to make assertions easier.

## UglyToad.PdfPig - Latest
Used to convert PDF to HTML.

## OpenAi - Latest
Used to call the OpenAI API service for converting text such as HTML to a SavedRecipe.
