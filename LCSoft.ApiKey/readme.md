# LCSoft: ApiKey Authorization for .NET 

This library provides a flexible, extensible, and resilient API key authorization system for .NET applications. Built with the strategy pattern at its core, it offers multiple integration approaches while maintaining clean separation of concerns and robust error handling.

[![NuGet](https://img.shields.io/nuget/v/LCSoft.ApiKey.svg)](https://www.nuget.org/packages/LCSoft.ApiKey)
[![License: CC BY-NC-ND 4.0](https://img.shields.io/badge/License-CC_BY--NC--ND_4.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc-nd/4.0/)

---

## Design Philosophy

The library is designed around three core principles:

**Flexibility:** Multiple integration options including attributes, middleware, endpoint filters, and authorization/authentication policies to fit any application architecture.

**Extensibility:** Strategy-based validation system that allows custom validation logic while maintaining a consistent interface.

**Resilience:** Built-in fallback mechanisms and comprehensive error handling to ensure your application remains stable even when configuration issues occur.

## Features  

- ✅ Full-featured API key authorization framework
- ✅ Easy integration with `.NET` (6.0+)  
- ✅ Lightweight and extensible  
- ✅ Supports API key validation through:  
   - Attributes 
   - Endpoint Filter  
   - Middleware
   - Authorization Policy
   - Authentication Policy
- ✅ Support for custom header names    
- ✅ Flexible setup with dependency injection  
- ✅ Extensible validation strategies
- ✅ Easy configuration

### Implemented Patterns
 - Strategy Pattern ✅
 - Factory Pattern (Service Factory) ✅
 - Dependency Injection Pattern ✅
 - Options Pattern (via IOptions) ✅

---

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package LC.ApiKey
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package LC.ApiKey
```

## Setup

This library integrates seamlessly with both MVC controllers and minimal APIs. Use it for quick API key authorization setup or integrate it as a standard policy for API key authentication.

### 1. Add as basic services

In your `Program.cs`, register the services, this will register the authorization filter to use with the [ApiKey] attribute and the IApiKeyValidator interface and service that will read the valid api key from appsettings configurations:

This will work only when using Controllers.

```csharp
builder.Services.RegisterApiKeyFilterAuthorization();
```

To do the same thing with minimal api you an use directly ApiKeyEndpointFilter as a Endpoint filter:

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

But then you need to register the IApiKeyValidator interface and the implementation, you can use the standard registring:

```csharp
builder.Services.RegisterApikeyServices();
```

### 2. Using as custom middleware
You can use as custom middleware, this will work with controllers and minimal apis
Use after app.UseHttpsRedirection(); and before any routing middleware.

```csharp
app.UseMiddleware<ApiKeyMiddleware>();
```

This will work with [AllowAnonymous] attribute and .AllowAnonymous() extension as well, any controller, action or endpoint marked as allow anonymous will be respected.

But then you need to register the IApiKeyValidator interface and the implementation, you can use the standard registring:

```csharp
builder.Services.RegisterApikeyServices();
```

### 3. Add the Authorization Policy

You can also add as a authorization policy, this options is the most flexible for this version, you can use just:

```csharp
builder.Services.RegisterApiKeyPolicyAuthorization();
```

This option will by default use the appsetting configuration as default, as well registring the basic IApiKeyValidator interface and implementation. For this version, this will get the configuration from "ApiKey" section, for example: {"ApiKey": "Your_Choosen_APIKEY"}.

To register with more than one option of APIKEY, you can pass the list of allowed apikeys:

```csharp
var validApiKeys = List<string>() {"validapikey1", "validapikey2", "validapikey3"}
builder.Services.RegisterApiKeyPolicyAuthorization(validApiKeys);
```

You also need to add the authorization and authentication middleware:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```


## How It Works

- The package inspects the specified header (e.g., `X-API-KEY`)  
- If the key is missing or invalid, access is denied  
- You can easily extend the validation logic by implementing `IApiKeyValidator`  
- The valid api key is unique and read from appsettings configuration the "ApiKey" section.

## Customization

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

###  Troubleshooting

Error: Invalid API Key → Verify the key matches in requests.
Missing Header → Ensure clients send X-API-Key.

## Support

## License

This package is licensed under CC BY-NC-ND 4.0.

## Contribute 
Found a bug? Want a feature?
Open an Issue or submit a PR!