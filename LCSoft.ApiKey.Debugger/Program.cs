// Using Controllers:

// 1 - Using a Custom Api Key Attribute
//#define WITHCONTROLLER_CustomApiKeyAttribute
//#define WITHCONTROLLER

// 2 - Using ApiKeyOrCustomAuthorizationAttribute
//#define WITHCONTROLLER_ApiKeyOrCustomAuthorizationAttribute
//#define WITHCONTROLLER

// 3 - Using Endpoints with Filter
//#define WITHENDPOINTS

// 4 - Using Middleware
//#define WITHMIDDLEWARE

// 5 - Using Policy for Authorization
//#define WITHPOLICY_AUTHORIZATION
//#define WITHCONTROLLER
//#define MINIMALAPI

// 6 - Using Policy for Authentication
#define WITHPOLICY_AUTHENTICATION
#define WITHCONTROLLER
//#define MINIMALAPI
using LCSoft.ApiKey.Debugger.AttributeVersion;
using LCSoft.ApiKey.Extensions;
using LCSoft.ApiKey.Debugger.EndpointFilterVersion;
using LCSoft.ApiKey.Debugger.Endpoints;
using LCSoft.ApiKey.Debugger.MiddlewareVersion;
using LCSoft.ApiKey.Debugger.PolicyVersion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

#if WITHCONTROLLER_CustomApiKeyAttribute
builder.Services.AddControllers();
builder.Services.RegisterApikeyServices();
#endif

#if WITHCONTROLLER_ApiKeyOrCustomAuthorizationAttribute
builder.Services.RegisterUsingAttributes(false);
#endif

#if WITHENDPOINTS
builder.Services.RegisterUsingEndointsFilter();
#endif

#if WITHMIDDLEWARE
builder.Services.RegisterUsingMiddleware();
#endif

#if WITHPOLICY_AUTHORIZATION
builder.Services.RegisterUsingPolicy();

#if WITHPOLICY_AUTHORIZATION && WITHCONTROLLER
builder.Services.AddControllers();
#endif
#endif

#if WITHPOLICY_AUTHENTICATION
builder.Services.RegisterUsingPolicyAuth(builder.Configuration);

#if WITHPOLICY_AUTHENTICATION && WITHCONTROLLER
builder.Services.AddControllers();
#endif
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

#if WITHENDPOINTS
app.MapTestEndpoints();
#endif

#if WITHCONTROLLER
app.MapControllers();
#endif

#if WITHMIDDLEWARE
app.UsingMiddleware();

app.MapGet("/UsingMiddleware", () =>
{
    var forecasts = new[]
    {
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 25, "Sunny"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 20, "Cloudy"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 15, "Rainy")
    };
    return forecasts;
});
#endif

#if WITHPOLICY_AUTHORIZATION
// Testing with Controllers

// Testing with MinimalApis
#if WITHPOLICY_AUTHORIZATION && MINIMALAPI
app.MapGet("/UsingAuthorizationPolicy", () =>
{
    var forecasts = new[]
    {
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 25, "Sunny"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 20, "Cloudy"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 15, "Rainy")
    };
    return forecasts;
}).RequireAuthorization();
#endif
#endif

#if WITHPOLICY_AUTHENTICATION
// Testing with Controllers

// Testing with MinimalApis
#if WITHPOLICY_AUTHENTICATION && MINIMALAPI
app.MapGet("/UsingAuthorizationPolicy", () =>
{
    var forecasts = new[]
    {
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), 25, "Sunny"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 20, "Cloudy"),
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 15, "Rainy")
    };
    return forecasts;
}).RequireAuthorization();
#endif
#endif

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}