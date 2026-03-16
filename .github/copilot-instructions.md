# Copilot Instructions for Money Spending Tracker

## Project Overview
Money Spending Tracker is a .NET MAUI Android application for tracking personal financial transactions with AI-powered tagging and categorization. The app uses bank account integration, local encrypted SQLite database, and machine learning for transaction categorization.

## Technology Stack
- **Framework**: .NET 10.0 (MAUI for Android)
- **Target Platform**: Android (minimum API 28)
- **Database**: SQLite with SQLCipher encryption (Microsoft.EntityFrameworkCore.Sqlite)
- **MVVM Framework**: CommunityToolkit.Mvvm
- **UI Toolkit**: CommunityToolkit.Maui, Syncfusion.Maui.Toolkit
- **Authentication**: Plugin.Fingerprint (biometric authentication)
- **Machine Learning**: Microsoft.ML.OnnxRuntime, Microsoft.ML.Tokenizers (BERT-based embeddings)
- **Background Jobs**: Xamarin.AndroidX.Work.Runtime

## Architecture & Patterns

### Project Structure
The solution contains two projects:
1. **MoneySpendingTracker** - Main MAUI application
2. **MoneySpendingTracker.Data** - Data layer with EF Core entities and migrations

### Feature Organization
All features are organized under `Money Spending Tracker\Features\` with one folder per feature:
- Each feature contains its own ViewModels, Pages, Models, and Services
- Features: Accounts, Api, BackgroundJob, ChromeTabs, Database, Home, MonthSummary, Onboarding, PreviousMonths, Settings, Start, Storage, Tags, TransactionData, Transactions

### Naming Conventions
- **Namespace**: Underscores in root namespace (`Money_Spending_Tracker`)
- **Files/Classes**: PascalCase
- **Private fields**: `_camelCase` with underscore prefix
- **ViewModels**: End with `ViewModel` suffix
- **Pages**: End with `Page` suffix (XAML + code-behind)
- **Models**: End with `Model` suffix
- **Services**: End with `Service` suffix
- **Popups**: End with `Popup` suffix

### MVVM Patterns
- Use `CommunityToolkit.Mvvm` attributes:
  - `[ObservableProperty]` for properties
  - `[RelayCommand]` for commands
  - Inherit from `ObservableObject` for ViewModels
- ViewModels should be marked `internal partial class`
- Use `x:DataType` in XAML for compiled bindings

### Code Style
- **Nullable Reference Types**: Enabled - use `?` appropriately
- **Implicit Usings**: Enabled
- **Comments**: Minimal - only for complex logic, XML docs for public APIs
- **Access Modifiers**: Prefer `internal` for non-public types within the main project
- Use `partial` classes for code-behind and ViewModels with source generators

### Dependency Injection
- Services registered in `MauiProgram.cs`
- Use constructor injection
- Pages/ViewModels registered as Transient
- Services typically Singleton or Transient based on use case
- Access via `MauiServiceProvider.Current` when needed outside DI

### Database
- **Entity Framework Core** with SQLite
- **Encryption**: SQLCipher for database encryption
- **Migrations**: Located in `Money Spending Tracker.Data\Migrations\`
- **DbContext**: `AppDbContext` - access via `DatabaseService`
- **Entities**: Located in Data project root
- Composite primary keys used (e.g., `InternalTransactionId` + `AccountId`)
- Navigation properties for relationships

### Platform-Specific Code
- Android-specific code in `Platforms\Android\` folder
- Conditional compilation: `#if ANDROID`
- Custom implementations for platform services (e.g., `ICustomTabService`, `IBackgroundService`)

### UI Patterns
- **Shell Navigation**: `AppShell` for navigation structure
- **Route Registration**: Routes registered in `AppShell` constructor
- **Query Parameters**: Use `[QueryProperty]` attribute with string serialization
- **Popups**: CommunityToolkit.Maui popups for modals
- **Widgets**: Android home screen widgets in `Platforms\Android\Widgets\`

### Machine Learning
- **Tag Prediction**: Uses ONNX Runtime with BERT tokenizer
- **Embeddings**: Stored as `byte[]` in database
- **Model**: Loaded from app package (`vocab.txt`)
- Cosine similarity for transaction matching
- Service pattern: `TagPredictionService` (implements `IDisposable`)

### Security
- Biometric authentication (fingerprint) for database unlock
- Encrypted SQLite database with password stored in `SecureStorage`
- API tokens with refresh logic in `ClientBase`

### Error Handling
- Global exception handler: `MauiExceptions.UnhandledException`
- Display alerts for unhandled exceptions
- Use `Debug.WriteLine` for logging

## Coding Guidelines

### When Adding New Features
1. Create a new folder under `Features\` with the feature name
2. Add ViewModel (if needed), Page (XAML + code-behind), Models, and Services
3. Register pages and services in `MauiProgram.cs`
4. Register routes in `AppShell.xaml.cs` if navigable
5. Follow the existing MVVM pattern with CommunityToolkit.Mvvm

### When Modifying Database
1. Add/modify entities in the Data project
2. Update `AppDbContext` if needed (DbSets, relationships)
3. Create a new migration: `Add-Migration <MigrationName>` in Data project
4. Test migration applies correctly

### When Adding UI
- Use compiled bindings with `x:DataType`
- Prefer XAML for UI layout
- Use resource dictionaries for colors/styles (in `Resources\Styles\`)
- Font Awesome icons available (Solid/Regular)

### When Adding Platform Features
- Put platform-specific code in `Platforms\<Platform>\`
- Use interfaces for cross-platform abstraction
- Register platform implementations in `MauiProgram.cs` with `#if` directives

### Background Jobs (Android)
- Use `WorkManager` via `Xamarin.AndroidX.Work.Runtime`
- Job workers in `Platforms\Android\`
- Configuration in Android manifest

## Common Patterns to Follow

### Service Pattern
```csharp
internal class MyService
{
    private readonly DatabaseService _databaseService;
    
    public MyService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
}
```

### ViewModel Pattern
```csharp
internal partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _myProperty;
    
    [RelayCommand]
    private async Task MyActionAsync()
    {
        // Implementation
    }
}
```

### Navigation with Parameters
```csharp
// In sending page/viewmodel:
await Shell.Current.GoToAsync($"{nameof(DetailPage)}?{nameof(DetailPage.ItemId)}={item.Id}");

// In receiving page:
[QueryProperty(nameof(ItemId), nameof(ItemId))]
public partial class DetailPage : ContentPage
{
    public string ItemId { get; set; }
}
```

### API Client Pattern
- Partial class `Client` with custom `PrepareRequestAsync` for auth
- Token management with refresh logic
- Base URL and endpoints defined

## Testing
- No unit tests currently in solution
- Manual testing on Android devices/emulators

## Performance Considerations
- Use async/await for I/O operations
- Database queries should use async methods
- ML inference can be CPU-intensive - consider background threads
- Widget updates should be efficient

## Important Notes
- **DO NOT** suggest Xamarin.Forms - this is a .NET MAUI project
- **DO** use .NET 10 APIs and MAUI-specific patterns
- **AVOID** adding unnecessary dependencies
- **FOLLOW** the existing feature folder structure strictly
- **USE** the existing DI container; don't create manual instances
- **ENSURE** null safety with nullable reference types
- **MAINTAIN** the underscored namespace convention (`Money_Spending_Tracker`)
- **RESPECT** the encrypted database - always use `DatabaseService` for access

## Resources
- App uses Font Awesome 6 (Free Solid & Regular)
- Custom fonts in `Resources\Fonts\`
- App icon and splash screen configured
- ONNX model (`vocab.txt`) in app package
