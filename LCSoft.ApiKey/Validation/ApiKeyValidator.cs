using LCSoft.ApiKey.Models;
using LCSoft.Results;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace LCSoft.ApiKey.Validation;

internal class ApiKeyValidator : IApiKeyValidator
{
    private readonly IApiKeyValidationStrategy _strategy;

    public ApiKeyValidator(
        IApiKeyValidationStrategyFactory factory,
        IOptions<ApiSettings> options)
    {
        _strategy = factory.Create(options.Value.StrategyType!);
    }

    public Results<bool> IsValid(string apiKey) => _strategy.IsValid(apiKey);

    public Results<ClaimsPrincipal> ValidateAndGetInfo(string apiKey)
        => _strategy.ValidateAndGetInfo(apiKey);
}