# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-01-17

### Added
- Initial release of LCSoft.ApiKey library
- Multiple integration options for API key authorization:
  - Controller attributes (`[ApiKey]`, `[CustomApiKey]`, `[CustomAuthorization]`)
  - Minimal API endpoint filters (`ApiKeyEndpointFilter`)
  - Middleware support (`ApiKeyMiddleware`)
  - Authorization policies
  - Authentication policies
- Strategy pattern implementation for extensible validation logic
- Built-in configuration support through `appsettings.json`
- Support for custom header names (defaults to `X-API-Key`)
- Support for Authorization header with `ApiKey` prefix
- Comprehensive service registration methods:
  - `RegisterApiKeyFilterAuthorization()`
  - `RegisterApikeyServices()`
  - `RegisterApiKeyCustomAuthorization()`
  - `RegisterApiKeyEndpointFilter()`
  - `RegisterApiKeyMiddleware()`
  - `RegisterApiKeyPolicyAuthorization()`
  - `RegisterApiKeyPolicyAuthentication()`
- Extension methods for minimal APIs:
  - `RequireApiKey()`
  - `AddEndpointFilter<ApiKeyEndpointFilter>()`
- Interfaces for custom implementations:
  - `IApiKeyValidator`
  - `IApiKeyValidationStrategy`
- Built-in validation strategies with fallback mechanisms
- Support for `[AllowAnonymous]` attribute and `.AllowAnonymous()` extension
- Comprehensive error handling and resilience features
- .NET 6.0+ compatibility
- Dependency injection integration
- CC BY-NC-ND 4.0 license

### Features
- **Flexible Integration**: Works with both MVC controllers and minimal APIs
- **Extensible Validation**: Custom validation logic through strategy pattern
- **Configuration-Driven**: Easy setup through appsettings.json
- **Multiple Authentication Methods**: Support for various header formats
- **Robust Error Handling**: Built-in fallback mechanisms
- **Clean Architecture**: Separation of concerns with dependency injection
- **Developer-Friendly**: Simple setup with comprehensive documentation

### Technical Details
- Target Framework: .NET 6.0+
- Package Type: NuGet Library
- License: Creative Commons BY-NC-ND 4.0
- Architecture: Strategy pattern with dependency injection
- Configuration: JSON-based with extensible options

[0.1.0]: https://github.com/luigi-sw/LCSoft.ApiKey/releases/tag/v0.1.0