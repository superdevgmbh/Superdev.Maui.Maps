# Superdev.Maui.Maps
[![Version](https://img.shields.io/nuget/v/Superdev.Maui.Maps.svg)](https://www.nuget.org/packages/Superdev.Maui.Maps) [![Downloads](https://img.shields.io/nuget/dt/Superdev.Maui.Maps.svg)](https://www.nuget.org/packages/Superdev.Maui.Maps) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)


### Download and Install Superdev.Maui.Maps
This library is available on NuGet: https://www.nuget.org/packages/Superdev.Maui.Maps
Use the following command to install Superdev.Maui.Maps using NuGet package manager console:

    PM> Install-Package Superdev.Maui.Maps

You can use this library in any .NET MAUI project compatible to .NET 9 and higher.

#### App Setup
1. This plugin provides an extension method for MauiAppBuilder `UseSuperdevMauiMaps` which ensure proper startup and initialization.
   Call this method within your `MauiProgram` just as demonstrated in the [MapsDemoApp](https://github.com/superdevgmbh/Superdev.Maui.Maps/tree/develop/Samples):
   ```csharp
   var builder = MauiApp.CreateBuilder()
       .UseMauiApp<App>()
       .UseSuperdevMauiMaps();
   ```
2. tbd

### Sample App
In the **Samples** folder of this repository, you will find the **MapsDemoApp**, which demonstrates the features of Superdev.Maui.Maps. To debug, clone the repository and run the sample app directly in your development environment.


### API Usage
The following documentation guides you trough the most important use cases of this library. Not all aspects are covered. If you think there is something important missing here, feel free to open a new issue.

This documentation only demonstrates the use of Superdev.Maui.Maps within a XAML and MVVM based app. Of course, the code also runs in C# and code-behind UIs.

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

### Links
- tbd
