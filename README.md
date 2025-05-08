# LC ApiKey Authorization for .NET 🔑
 
*A simple and secure way to add API key authorization to your .NET APIs.*

[![NuGet](https://img.shields.io/nuget/v/Your.Package.Name.svg?style=flat-square)](https://www.nuget.org/packages/Your.Package.Name)  
[![License: CC BY-NC-ND 4.0](https://img.shields.io/badge/License-CC_BY--NC--ND_4.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc-nd/4.0/)

---

## 🚀 Features ✨  

- ✅ Simple API key validation for ASP.NET Core Web APIs
- ✅ Easy integration with `ASP.NET Core` (6.0+)  
- ✅ Lightweight and extensible  
- ✅ Supports API key validation via:  
   - Header (`X-API-Key`)  
   - Query string (`?apiKey=...`)  
   - Custom providers  
- ✅ Support for custom header names  
- ✅ Optional in-memory or configurable key storage  
- ✅ Plug-and-play `IAuthorizationHandler` integration  
- ✅ Minimal setup required  

---

## 📦 Installation

Install via NuGet Package Manager:

```bash
dotnet add package ApiKeyAuthorization
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package ApiKeyAuthorization
```

## 🛠️ Setup

### 1. Add Middleware and Services

In your `Program.cs`, register the services:

```csharp
builder.Services.AddApiKeyAuthorization(options =>
{
    options.HeaderName = "X-API-KEY";
    options.ValidApiKeys = new[] { "your-api-key-1", "your-api-key-2" };
});
```

### 2. Add the Authorization Policy

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>
    {
        policy.Requirements.Add(new ApiKeyRequirement());
    });
});
```

### 3. Apply to Controllers or Actions

```csharp
[Authorize(Policy = "ApiKeyPolicy")]
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSecretData()
    {
        return Ok("This is protected data.");
    }
}
```

### 4. Using with Minimal APIs
When using with your minimal apis, you get the option to inject into the authentication/authorization middleware and use as endpoint filter

```csharp
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.AddEndpointFilter<ApiKeyEndpointFilter>()
.WithName("GetWeatherForecast");
```

### 5. Using as custom middleware
You can use as custom middleware, this will work with controllers and minimal apis
Use after app.UseHttpsRedirection(); and before any routing middleware.

```csharp
app.UseMiddleware<ApiKeyMiddleware>();
```

This will work with [AllowAnonymous] attribute and .AllowAnonymous() extension as well, any controller, action or endpoint marked as allow anonymous will be respected.


## 🔐 How It Works

- The package inspects the specified header (e.g., `X-API-KEY`)  
- If the key is missing or invalid, access is denied  
- You can easily extend the validation logic by implementing `IApiKeyValidator`  

## 💡 Customization

Implement your own validation logic:

```csharp
public class CustomApiKeyValidator : IApiKeyValidator
{
    public bool IsValid(string apiKey)
    {
        // Add your logic here, e.g. check database or cache
        return Task.FromResult(apiKey == "your-dynamic-key");
    }
}
```

Then register it:

```csharp
builder.Services.AddSingleton<IApiKeyValidator, CustomApiKeyValidator>();
```

## Advanced Configuration ⚙️
Custom Validation Logic
Override IApiKeyValidator:

```csharp
builder.Services.AddSingleton<IApiKeyValidator, CustomApiKeyValidator>();
```
Environment-Based Keys
Store keys in appsettings.json:

```json
{
  "ApiKeySettings": {
    "ApiKey": "Production-Key-Here"
  }
}
```
Then bind it:

```csharp
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection("ApiKeySettings"));
```

###  Troubleshooting 🔧

Error: Invalid API Key → Verify the key matches in requests.
Missing Header → Ensure clients send X-API-Key.

## 📄 License

This package is licensed under CC BY-NC-ND 4.0.
Full License Text

## 🙋 Support

## Contribute ❤️
Found a bug? Want a feature?
Open an Issue or submit a PR!