//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System.Text.Encodings.Web;

//namespace LC.ApiKey.Policy.Auhtorization;

//internal class NullAuthHandlerAdapter : AuthenticationHandler<AuthenticationSchemeOptions>
//{
//    private readonly NullAuthHandler2 _inner;

//    public NullAuthHandlerAdapter(
//        IOptionsMonitor<AuthenticationSchemeOptions> options,
//        ILoggerFactory logger,
//        UrlEncoder encoder,
//        ISystemClock clock,
//        NullAuthHandler2 inner)
//        : base(options, logger, encoder, clock)
//    {
//        _inner = inner;
//    }

//    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
//    {
//        await _inner.InitializeAsync(Scheme, Context);
//        return await _inner.AuthenticateAsync();
//    }

//    //protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
//    //{
//    //    await _inner.InitializeAsync(Scheme, Context);
//    //    await _inner.ChallengeAsync(properties);
//    //}

//    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
//    {
//        await _inner.InitializeAsync(Scheme, Context);
//        await _inner.ForbidAsync(properties);
//    }
//}
