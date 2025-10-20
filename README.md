# Denomica.Odata

A .NET Standard 2.1 library that provides functionality for building applications that support OData.

## Installation

Install the package via NuGet:

```bash
dotnet add package Denomica.Odata
```

Or via the Package Manager Console:

```powershell
Install-Package Denomica.Odata
```

## Requirements

- .NET Standard 2.1 or higher

## Development

### Building the Library

To build the library:

```bash
dotnet build
```

### Creating the NuGet Package

To create the NuGet package:

```bash
dotnet pack -c Release
```

The package will be created in `src/Denomica.Odata/bin/Release/`.

### Project Structure

```
Denomica.Odata/
├── src/
│   └── Denomica.Odata/
│       ├── Denomica.Odata.csproj
│       ├── ODataLibrary.cs
│       └── readme.md
├── Denomica.Odata.sln
├── README.md
└── LICENSE
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
