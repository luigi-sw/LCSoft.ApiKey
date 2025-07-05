using LC.ApiKey.Extensions;
using LC.ApiKey.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


//OPT5
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));


//builder.Services.AddAuthorizationBuilder()
//                .AddPolicy("ApiKeyOrBearer", policy =>
//                {
//                    policy.AddAuthenticationSchemes("Bearer", "ApiKey");
//                    policy.RequireAuthenticatedUser();
//                });

//builder.Services.AddControllers(options =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();
//    options.Filters.Add(new AuthorizeFilter(policy));
//});

builder.Services.RegisterApikeyServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//OPT2b
//app.UseMiddleware<ApiKeyMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

//app.UseAuthentication();
//app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "Hello World!");

//[ApiKey] OPT1 OPT2a
//[Authorize(Policy = "ApiKeyPolicy")] OPT2d
//[Authorize("ApiKeyOrBearer")] OPT6
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
//.AllowAnonymous()
//.RequireAuthorization()
//.AddEndpointFilter<ApiKeyEndpointFilter>() //OPT2c
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}