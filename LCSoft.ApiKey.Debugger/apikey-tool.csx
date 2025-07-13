#nullable enable
#r "nuget:System.Text.Json"

using System;
using System.Text;
using System.Text.Json;

public class ApiKeyInfo
{
    public string Key { get; set; } = "";
    public string Owner { get; set; } = "";
    public string[]? Roles { get; set; }
    public string[]? Scopes { get; set; }
}

var mode = Args.Count > 0 ? Args[0].ToLowerInvariant() : "help";

switch (mode)
{
    case "generate":
        Generate();
        break;
    case "validate":
        Validate();
        break;
    default:
        ShowHelp();
        break;
}

void Generate()
{
    Console.Write("Key: ");
    var key = Console.ReadLine();

    Console.Write("Name: ");
    var name = Console.ReadLine();

    Console.Write("Roles (comma-separated): ");
    var roles = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    Console.Write("Scopes (comma-separated): ");
    var scopes = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    var info = new ApiKeyInfo
    {
        Key = key ?? "",
        Owner = name ?? "",
        Roles = roles,
        Scopes = scopes
    };

    var json = JsonSerializer.Serialize(info);
    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

    Console.WriteLine("\nGenerated API Key (Base64):");
    Console.WriteLine(base64);
}

void Validate()
{
    Console.Write("Paste Base64 API Key: ");
    var encoded = Console.ReadLine();

    try
    {
        var bytes = Convert.FromBase64String(encoded ?? "");
        var json = Encoding.UTF8.GetString(bytes);

        var info = JsonSerializer.Deserialize<ApiKeyInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Console.WriteLine("\nDecoded API Key Info:");
        Console.WriteLine(JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void ShowHelp()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet script apikey-tool.csx generate   # Create new Base64 API key");
    Console.WriteLine("  dotnet script apikey-tool.csx validate   # Decode existing Base64 API key");
}