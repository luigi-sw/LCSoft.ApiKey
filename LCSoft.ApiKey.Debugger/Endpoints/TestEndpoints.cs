namespace LCSoft.ApiKey.Debugger.Endpoints;

public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

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
    }
}
