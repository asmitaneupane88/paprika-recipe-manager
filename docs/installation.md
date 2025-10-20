# Installation Guide

This guide explains how to set up the devepment enviroment for the Paprika Recipe Manager, a .NET-based application using the Uno platform for cross-platform UI, on Windows. This setup supports the core features of Sprint 1, including recipe mangement and MealDB API integration.

## Prerequisites
- **System Requirements**:
    - Windows 10 or 11 (64-bit)
    - 8GB RAM (16GB recommended)
    - 10GB free disk space
- **Software**:
    - [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
    - [Git](https://git-scm.com/downloads)
    - IDE (choose one):
        - [JetBrains Rider](https://www.jetbrains.com/rider/) (recommended)
        - [Visual Studio 2022](https://visualstudio.microsoft.com/) with ".NET Desktop Development" and "Universal Windows Platform development" workloads
        - [Visual Studio Code](https://code.visualstudio.com/) with C# extension
- **Uno Platform Extension**:
    - Required for all IDEs to support Uno Platform development.
## Setup Instructions
1. **Install Development Tools**
- Download and install .NET 9 SDK from the [official site](https://dotnet.microsoft.com/download/dotnet/9.0).
- Install Git for Windows from [git-scm.com](https://git-scm.com/downloads).
- Verify installations:
```bash
dotnet --version
git --version
  ```
2. **Install IDE**
- **JetBrains Rider**
  - Download and run the installer from [JetBrains](https://www.jetbrains.com/rider/)
  - Enable recommended plugins (e.g, .NET, Uno Platform) during setup.
- **Visual Studio**:
  - Download the installer from [Microsoft](https://visualstudio.microsoft.com/)
  - Select ".NET Desktop Development" and "Universal Windows Platform development" workloads.
- **Visual Studio Code**:
  - Install from [code.visualstudio.com](https://code.visualstudio.com/)
  - Add the C# extension via the Extensions panel.
3. **Install Uno Platform**:
- Install the Uno.Check tool
    ```bash
  dotnet tool install --gobal Uno.check
    ```
- Install the Uno Platform Extension:
  - **Rider**: Go to Settings -> Pluggins, search for "Uno Platform", install and restart.
  - **Visual Studio**: Go to Extension -> Manage Extensions, search for "Uno Platform", install, and restart.
  - **VS Code**: Open Extensions panel, search for "Uno Platform", install, and reload.
- Verify Uno Platform setup:
    ```bash
    uno -check
    ```
4. **Clone and Set Up the Project**
- **Clone the repository**:

   ```bash
   git clone https://github.com/asmitaneupane88/paprika-recipe-manager.git
   cd paprika-recipe-manager
   ```
- **Restore Dependencies**

   ```bash
   dotnet restore
   ```
- **Build Project**

   ```bash
   dotnet build
   ```
5. **Configure API Connectivity**
- **Purpose**: Ensure the application can connect to the **MealDB** API for recipe search functionality (handled by `Services/ApiControl.cs` and `ViewModels/SearchViewModel.cs`)
- **Steps**:
  1. **Verify Internet Access**:
     - Ensure a stable internet connection, as the MealDB API requires online access.
     - Test connectivity by pinging the API endpoint:
  ```bash
     ping www.themealdb.com
  ```
  2. **Check API Configuration**:
     - The MealDB API's free tier does not required an API key, and the current implementation hardcodes the API URL in `Services/ApiControl.cs`.
  3. **Test API Connectivity**:
     - Run the application (`dotnet run --project ReceipeApp/RecipeApp.csproj`)
     - Navigate to the search page (`Controls/Pages/MealDbSearchPage`) and perform a test search (e.g., "chicken")
     - Verify that `SearchViewModel.cs` polulates `SearchResults` with data from `MealDBRecipe.cs`.

## Troubleshooting
### Build Errors:
- Ensure .NET 9 SDK is install `(dotnet --version)`.
- Clear NuGet cache `dotnet nuget locals all --clear`.
- Rebuild: `dotnet clean && dotnet build`.
### Uno Platform Issues:
- Run `uno-check` to diagnose missing depedencies.
- Reinstall the Uno Platform extension if UI components fail to load.
### API Connectivity:
1. **No Response from MealDB API**:
   - Verify internet connectivity (`ping www.themealDB.com`)
   - Check API status by accessing `https://www.themealdb.com/api/json/v1/1/search.php?s=test` in the browser.
2. **Search Results Not Displaying**:
   - Confirm that `MealDbSearchPage.xaml` bindings match `SearchViewModel` properties.
   - Check `ApiControl.cs` for deserialization issues.
### IDE issues:
- Update IDE to the latest version.
- Clear IDE cache and restart.
