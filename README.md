# Superdev.Maui.Maps
[![Version](https://img.shields.io/nuget/v/Superdev.Maui.Maps.svg)](https://www.nuget.org/packages/Superdev.Maui.Maps) [![Downloads](https://img.shields.io/nuget/dt/Superdev.Maui.Maps.svg)](https://www.nuget.org/packages/Superdev.Maui.Maps) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)


## Download and Install Superdev.Maui.Maps
This library is available on NuGet: https://www.nuget.org/packages/Superdev.Maui.Maps
Use the following command to install Superdev.Maui.Maps using NuGet package manager console:

    PM> Install-Package Superdev.Maui.Maps

You can use this library in any .NET MAUI project compatible to .NET 9 and higher.

## App Setup
1. This plugin provides an extension method for MauiAppBuilder `UseSuperdevMauiMaps` which ensure proper startup and initialization.
   Call this method within your `MauiProgram` just as demonstrated in the [MapsDemoApp](https://github.com/superdevgmbh/Superdev.Maui.Maps/tree/develop/Samples):
   ```csharp
   var builder = MauiApp.CreateBuilder()
       .UseMauiApp<App>()
       .UseSuperdevMauiMaps();
   ```
2. tbd

## Sample App
In the **Samples** folder of this repository, you will find the **MapsDemoApp**, which demonstrates the features of Superdev.Maui.Maps. To debug, clone the repository and run the sample app directly in your development environment.


## API Usage
The following documentation guides you through the most important use cases of this library.  
Not all aspects are covered. If you think there is something important missing here, feel free to open a new issue.

This documentation only demonstrates the use of Superdev.Maui.Maps within a XAML and MVVM-based app. Of course, the code also works in C# and code-behind UIs.

### Map 🗺
The `Map` control is the core component of **Superdev.Maui.Maps**.  
`Superdev.Maui.Maps.Controls.Map` replaces the well-known `Microsoft.Maui.Controls.Maps.Map` and extends it with additional **bindable properties**, **MVVM-friendly features**, and additional functionality for **templated pins**, **two-way map region binding**, and **custom interaction handling**.

You can use `Map` directly in XAML and bind to its properties like any other MAUI control. Don't forget to import the correct XAML namespace alias in order to use the map controls of this library.
```xaml
xmlns:m="http://Superdev.Maui.Maps"
```

Example:
```xaml
<m:Map
    ItemsSource="{Binding Locations}"
    SelectedItem="{Binding SelectedLocation, Mode=TwoWay}"
    VisibleRegion="{Binding VisibleRegion, Mode=TwoWay}"
    IsShowingUser="True" />
```

#### Bindable Properties

| Property                         | Description                                                                                                                                                                                         |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `IsShowingUser`                  | Shows an indicator for the user’s current location. *(Default: false)*                                                                                                                              |
| `IsScrollEnabled`                | Enables or disables scrolling/panning by user input. *(Default: true)*                                                                                                                              |
| `IsZoomEnabled`                  | Enables or disables zooming by user input. *(Default: true)*                                                                                                                                        |
| `IsRotateEnabled`                | Enables or disables rotation of the map by user input. *(Default: true)*                                                                                                                            |
| `IsTiltEnabled`                  | Enables or disables tilting by user input. *(Default: true)*                                                                                                                                        |
| `IsTrafficEnabled`               | Displays a live traffic overlay. *(Default: false)*                                                                                                                                                 |
| `MapType`                        | Defines the visual style of the map (`Street`, `Satellite`, `Hybrid`, etc.).                                                                                                                        |
| `IsReadonly`                     | Makes the map pins non-interactive when `true`. *(Default: false)*                                                                                                                                  |
| `CenterPosition`                 | Sets or tracks the center `Location` of the map. Changing this property moves the map's viewport.                                                                                                   |
| `ZoomLevel`                      | Defines the zoom level as a `Distance`. *(Default: default(Distance))*                                                                                                                              |
| `VisibleRegion`                  | Defines the visible region (`MapSpan`) of the map (center + radius). You can either use `CenterPosition` + `ZoomLevel` to specify the current map position or you can use `VisibleRegion` to do so. |
| `ItemsSource`                    | The data collection used to generate pins. Don't forget  to specify an `ItemTemplate` or an `ItemTemplateSelector` to display pins.                                                                 |
| `SelectedItem`                   | Two-way: the currently selected item from the `ItemsSource`.                                                                                                                                        |
| `ItemTemplate`                   | Template used to render each element in `ItemsSource`.                                                                                                                                              |
| `ItemTemplateSelector`           | Template selector used to dynamically choose templates.                                                                                                                                             |
| `MapElements`                    | Collection of visual map overlays (pins, polylines, polygons, etc.) currently attached to the map.                                                                                                  |
| `MapClicked`                     | Event raised whenever the user taps on the map surface (not on a pin or element). Provides the tapped `Location` in `MapClickedEventArgs` as event args.                                            |
| `MapClickedCommand`              | Command executed whenever the user taps on the map surface (not on a pin or element). Provides the tapped `Location` in `MapClickedEventArgs` as command parameter.                                 |
| `DeselectSelectedItemOnMapClick` | When `true`, clicking an empty area of the map automatically clears the current `SelectedItem`. (Default: `false`)                                                                                    |

### Pin 📍
The `Pin` control represents a map marker.
`Superdev.Maui.Maps` extends the default `Microsoft.Maui.Controls.Maps.Pin` with additional bindable properties to support **custom icons**, **anchor positioning**, **selection tracking**, and **command binding** for marker interactions.

Example:
```xml
<m:Pin
    Label="{Binding Name}"
    Location="{Binding Coordinates}"
    ImageSource="{Binding Icon}"
    MarkerClickedCommand="{Binding PinClickedCommand}" />
```

#### Bindable Properties

| Property               | Description                                                                                              |
|------------------------|----------------------------------------------------------------------------------------------------------|
| `ImageSource`          | Custom: image or icon shown for this pin. Supports any MAUI `ImageSource`.                              |
| `Anchor`               | Custom: defines the anchor point of the pin image (e.g., `0.5, 1.0` anchors at the bottom center). *(Default: 0.5, 0.5)* |
| `MarkerClickedCommand` | Custom: command executed when the user taps the pin.                                                     |
| `IsSelected`           | Custom: indicates whether this pin is selected. One-way-to-source binding.                               |
| `Address`              | Inherited: address or description text associated with the pin.                                          |
| `Label`                | Inherited: label or title displayed for the pin.                                                         |
| `Location`             | Inherited: geographic position (latitude/longitude).                                                     |
| `Type`                 | Inherited: defines the type of pin (`Generic`, `Place`, etc.).                                           |

## Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

## Links
- https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map
- https://github.com/TorbenK/TK.CustomMap
- https://github.com/symbiogenesis/MauiMvvmMap
- https://github.com/RustaMSHar/MapsDemo
- https://github.com/TrashMob-eco/TrashMob
- https://github.com/ErNeRooo/AirsoftBmsApp
- https://github.com/VladislavAntonyuk/MauiSamples/tree/main/MauiMaps
- https://github.com/dmariogatto/Maui.Controls.BetterMaps
- https://github.com/CarlosMenendezSMSopen/VMedic
- https://github.com/iratrips-india/IratripsMapKit/tree/maui
- https://github.com/jorgediegocrespo/MauiMaps/tree/main
- https://github.com/dotnet/macios/blob/main/src/mapkit.cs
- https://github.com/NAXAM/cchmapclustercontroller-ios-binding/
- https://www.luftlinie.org
