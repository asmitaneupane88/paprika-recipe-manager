# Installation Guide

This guide provides detailed instructions for setting up the Paprika Recipe Manager development environment on both macOS and Windows.

## Prerequisites

### All Platforms

- .NET 9 SDK
- Git
- One of the supported IDEs:
  - JetBrains Rider (recommended)
  - Visual Studio
  - Visual Studio Code

## Uno Platform Setup

1. Install UnoCheck Command Line Tool

   ```bash
   dotnet tool install --global Uno.Check
   ```

2. Request Student License
   - Email Uno Platform for a free student license
   - Include your student email and institution details
   - This license enables:
     - Hot reloading functionality
     - UI designer features
     - Advanced debugging tools

## Platform-Specific Installation

### macOS Installation

1. **Install Development Tools**

   ```bash
   # Install Homebrew if not already installed
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

   # Install .NET SDK
   brew install --cask dotnet-sdk

   # Verify installation
   dotnet --version
   ```

2. **IDE Installation**

   - **JetBrains Rider (Recommended)**

     1. Download Rider from JetBrains website
     2. Install using the DMG file
     3. Launch Rider and install recommended plugins

   - **Visual Studio for Mac**
     1. Download from Microsoft's website
     2. Run the installer package
     3. Follow the installation wizard

3. **Uno Platform Extension**

   - In Rider:

     1. Go to Settings → Plugins
     2. Search for "Uno Platform"
     3. Install and restart IDE

   - In VS Code:
     1. Open Extensions panel
     2. Search for "Uno Platform"
     3. Install and reload

4. **Additional Tools**

   ```bash
   # Install additional development tools
   dotnet tool install --global Uno.Check

   # Run environment check
   uno-check
   ```

### Windows Installation

1. **Install Development Tools**

   - Download and install .NET 9 SDK from Microsoft
   - Install Git for Windows
   - Install Windows Terminal (recommended)

2. **IDE Installation**

   - **JetBrains Rider (Recommended)**

     1. Download Rider installer
     2. Run the installer with default settings
     3. Enable recommended plugins

   - **Visual Studio**
     1. Download Visual Studio installer
     2. Select ".NET Desktop Development"
     3. Include "Universal Windows Platform development"

3. **Uno Platform Extension**

   - In Rider:

     1. Settings → Plugins
     2. Market Place → Search "Uno Platform"
     3. Install and restart

   - In Visual Studio:
     1. Extensions → Manage Extensions
     2. Search for "Uno Platform"
     3. Download and install

4. **UWP Development (Windows Only)**
   1. Enable Developer Mode in Windows Settings
   2. Install Windows SDK
   3. Enable Hyper-V for emulator support

## Project Setup

1. **Clone Repository**

   ```bash
   git clone https://github.com/asmitaneupane88/paprika-recipe-manager.git
   cd paprika-recipe-manager
   ```

2. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

3. **Build Project**

   ```bash
   dotnet build
   ```

4. **Environment Verification**

   ```bash
   # Verify Uno Platform setup
   uno-check

   # Run the application
   dotnet run --project RecipeApp/RecipeApp.csproj
   ```

## Troubleshooting

### Common Issues

1. **Build Errors**

   - Verify .NET SDK version matches project requirements
   - Clear NuGet cache: `dotnet nuget locals all --clear`
   - Rebuild solution: `dotnet clean && dotnet build`

2. **IDE Issues**

   - Update IDE to latest version
   - Reinstall Uno Platform extension
   - Clear IDE cache and restart

3. **Runtime Errors**
   - Check UnoCheck output for missing dependencies
   - Verify environment variables
   - Update platform tools

## Documentation Resources

   - [Uno Platform Documentation](https://platform.uno/docs/articles/intro.html)
   - [Project Wiki](docs/introduction.md)
   - [Troubleshooting Guide](docs/getting-started.md)
