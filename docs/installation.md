# Installation Guide

This guide explains how to set up the development environment for the PseudoChef,
a .NET-based application using the Uno platform for cross-platform UI, on Windows.

## Prerequisites
- **System Requirements**:
    - Windows 10 or 11 (64-bit), MacOS, or Linux
    - 4GB of RAM (8GB recommended)
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
  - Use either the UI of the IDE or the terminal to clone the repository.
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

## Troubleshooting
### Build Errors:
- Ensure .NET 9 SDK is installed `(dotnet --version)`.
- Ensure that C# 14 is installed and supported by the IDE.
- Clear NuGet cache `dotnet nuget locals all --clear`.
- Rebuild: `dotnet clean && dotnet build`.
### Uno Platform Issues:
- Run `uno-check` to diagnose missing depedencies.
- Reinstall the Uno Platform extension if UI components fail to load.
### IDE issues:
- Update the IDE to the latest version.
- Clear IDE cache and restart.
