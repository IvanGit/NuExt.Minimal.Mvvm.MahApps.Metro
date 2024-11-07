# NuExt.Minimal.Mvvm.MahApps.Metro

`NuExt.Minimal.Mvvm.MahApps.Metro` is a NuGet package that provides extensions for integrating [MahApps.Metro](https://github.com/MahApps/MahApps.Metro), a popular Metro-style UI toolkit for WPF applications, with [NuExt.Minimal.Mvvm.Windows](https://github.com/IvanGit/NuExt.Minimal.Mvvm.Windows), an extension for the lightweight MVVM framework [NuExt.Minimal.Mvvm](https://github.com/IvanGit/NuExt.Minimal.Mvvm). This package includes services and components to facilitate the creation of modern, responsive, and visually appealing user interfaces using the MVVM pattern.

### Commonly Used Types

- **`Minimal.Mvvm.Windows.DialogCoordinatorService`**: Provides dialog coordination services using MahApps.Metro dialogs.
- **`Minimal.Mvvm.Windows.MetroDialogService`**: Implementation of `IAsyncDialogService` for Metro dialogs.
- **`Minimal.Mvvm.Windows.MetroTabbedDocumentService`**: Manages tabbed documents within a UI.
- **`MahApps.Metro.Controls.Dialogs.MetroDialog`**: The class used for custom dialogs.

### Key Features

The `MetroTabbedDocumentService` class is responsible for managing tabbed documents within a UI that utilizes the Metro design language. It extends the `DocumentServiceBase` and implements interfaces for asynchronous document management and disposal. This service allows for the creation, binding, and lifecycle management of tabbed documents within `MetroTabControl`.

### Installation

You can install `NuExt.Minimal.Mvvm.MahApps.Metro` via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.Minimal.Mvvm.MahApps.Metro
```

Or through the Visual Studio package manager:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm.MahApps.Metro`.
3. Click "Install".

### Usage Examples

For comprehensive examples of how to use the package, refer to the [samples](samples) directory in the repository. These samples illustrate best practices for using Minimal.Mvvm and MahApps.Metro with these extensions.

### Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and send pull requests. Your feedback and suggestions for improvement are highly appreciated.

### License

Licensed under the MIT License. See the LICENSE file for details.