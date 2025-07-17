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

- Full-featured API key authorization framework
- Easy integration with `.NET` (6.0+)  
- Lightweight and extensible  
- Supports API key validation through:  
   - Attributes 
   - Endpoint Filter  
   - Middleware
   - Authorization Policy
   - Authentication Policy
- Support for custom header names    
- Flexible setup with dependency injection  
- Extensible validation strategies
- Easy configuration

---

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package LCSoft.ApiKey
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package LCSoft.ApiKey
```

## Setup

This library integrates seamlessly with both MVC controllers and minimal APIs. Use it for quick API key authorization setup or integrate it as a standard policy for API key authentication.

### Usage Options

There are three main ways to use this library:
- **Using Controllers:** If your project is already using Controllers you can add using attributes or Custom Authorization
- **Using Minimal Apis:** If your project is using minimal APIs, you can use the `ApiKeyEndpointFilter` directly as an endpoint filter.
- **Using Middleware / Policy:** If you want a way to use the API Key Authorization as a middleware or policy and use with the functions already provided with .NET.

### 1. Using Controllers

When you are using controllers in your project, there are also three main ways to use the library:
- **Using Attribute [ApiKey]:** You can use the `[ApiKey]` attribute to protect your controllers or actions.
- **Using Attribute [CustomApiKey]:** You can use the `[CustomApiKey]` attribute to protect your controllers or actions with a custom header name.
- **Using Custom Authorization:** You can use [CustomAuthorization] attribute in your controller to protect your controllers with a custom authorization.

#### 1.1 Using Attribute [ApiKey]

To use ApiKey attribute, first step is to register the services using `RegisterApiKeyFilterAuthorization()` method in your service collection.

```csharp
builder.Services.RegisterApiKeyFilterAuthorization();
```

Then, you can use the `[ApiKey]` attribute on your controllers or actions:
```csharp
[HttpGet("GetApiKey")]
[ApiKey]
public IActionResult GetApiKey()
{
    return Ok("Api Key is valid and working!");
}
```

This version uses the `IAuthorizationFilter` on top of `ServiceFilterAttribute`. It's a Service Filter.

#### 1.2 Using Attribute [CustomApiKey]

To use CustomApiKey attribute, first step is to register the services using `RegisterApikeyServices()` method in your service collection.
```csharp
builder.Services.RegisterApikeyServices();
```

Then, you can use the `[CustomApiKey]` attribute on your controllers or actions, specifying the custom header name:
```csharp
[HttpGet("CustomApiKey")]
[CustomApiKey]
public IActionResult GetCustomApiKey()
{
    return Ok("Api Key is valid and working!");
}
```

This version uses `IAsyncActionFilter` on `Attribute`. It's an action filter.

#### 1.3 Using Custom Authorization [CustomAuthorization]

To use the `CustomAuthorization`, you can register the services using `RegisterApiKeyCustomAuthorization()` method in your service collection.

```csharp
builder.Services.RegisterApiKeyCustomAuthorization();
```

This version uses `IAuthorizationFilter` directly on `Attribute`. It's an Authorization Filter.

### 2. Using with Minimal APIs

If your project is using minimal APIs, you can use the `ApiKeyEndpointFilter` directly as an endpoint filter.

You can register the services using `RegisterApiKeyEndpointFilter()` method in your service collection.
```csharp
builder.Services.RegisterApiKeyEndpointFilter();
```
Then you can use the `ApiKeyEndpointFilter` in your minimal API endpoints like this:

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

Or use the `.RequireApiKey()` extension method:

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
.RequireApiKey()
.WithName("GetWeatherForecast");
```

### 3. Using as custom middleware or Policy

If you want to use the API Key Authorization that will work independently of whether it's controllers or minimal API, 
you can use in three ways:
- **Using as Middleware:** This will add a new middleware in the pipeline.
- **Using as Authorization Policy:** This will add a new authorization policy.
- **Using as Authentication Policy:** This will add a new authentication policy.

#### 3.1 Using as Middleware

You can use as custom middleware, this will work with controllers and minimal APIs
Use after app.UseHttpsRedirection(); and before any routing middleware.

```csharp
app.UseMiddleware<ApiKeyMiddleware>();
```

This will work with [AllowAnonymous] attribute and .AllowAnonymous() extension as well, any controller, action or endpoint marked as allow anonymous will be respected.

But then you need to register the IApiKeyValidator interface and the implementation, you can use the standard registering:

```csharp
builder.Services.RegisterApiKeyMiddleware();
```

#### 3.2 Using as Authorization Policy

You can also add as a authorization policy, This option is the most flexible for this version, you can use just:

```csharp
builder.Services.RegisterApiKeyPolicyAuthorization();
```

#### 3.3 Using as Authentication Policy

You can also add as a authentication policy, you can use just:
```csharp
builder.Services.RegisterApiKeyPolicyAuthentication(_ => {});
```

## How It Works

- The package inspects the specified header (e.g., `X-API-KEY`)  
- If the key is missing or invalid, access is denied  
- You can easily extend the validation logic by implementing `IApiKeyValidator`  
- The valid api key list of unique values are read from appsettings configuration the "ApiKey" section.

By default, it uses the appsettings configuration, as well as registering the basic IApiKeyValidator interface and implementation. For this version, this will get the configuration from "ApiKey" section, for example: 

```json
"ApiKey": {
    "Default": {
      "ApiKeys": [
        "apikey1",
        "apikey2",
        "apikey3"
      ]
    }
  }
```

You can also customize the header, By default it will use `X-API-Key`, but you can change it by changing the `appsettings.json`, for example:

```json
"ApiKey": {
    "HeaderName": "X-Api-Key"
  }
```

> Note: To use the "Authorization" header, you need to send in format: `Authorization: ApiKey your-api-key`

## Customization

Implement your own validation logic:

```csharp
public class CustomApiKeyValidator : IApiKeyValidator
{
    public  Results<bool> IsValid(string apiKey);
    {
        // Add your logic here, e.g. check database or cache
        return Results<bool>.Success(apiKey == "your-dynamic-key");
    }
}
```

Then register it:

```csharp
builder.Services.AddScoped<IApiKeyValidator, CustomApiKeyValidator>();
```

This library uses a strategy pattern for validation, allowing you to implement custom logic while maintaining a consistent interface.
For that, you can implement your custom `IApiKeyValidationStrategy`:

```csharp
public class CustomStrategyApiKeyValidator : IApiKeyValidationStrategy
{
    public string Name => "custom";

    public  Results<bool> IsValid(string apiKey);
    {
        // Add your logic here, e.g. check database or cache
        return Results<bool>.Success(apiKey == "your-dynamic-key");
    }
}
```

Then to register you can:
```csharp
builder.Services.AddScoped<IApiKeyValidationStrategy, CustomStrategyApiKeyValidator>()
```

Then you can set to use your custom strategy in your configuration:
```json
"ApiKey": {
    "StrategyType": "custom"
  }
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