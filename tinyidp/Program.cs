using tinyidp.Business.Credential;
using tinyidp.infrastructure.bdd;
using tinyidp.Encryption;
using Microsoft.AspNetCore.Authentication.Cookies;
using tinyidp.Business.keysmanagment;
using tinyidp.WebAuthent.Modules;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using tinyidp.Business.Certificate;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using tinyidp.Controllers;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tinyidp.Business.BusinessEntities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using tinyidp.Controllers.Models;
using System.Threading.RateLimiting;
using tinyidp.Business.tokens;
using tinyidp.Components;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/var/cache/secrets/tinyidp.db", optional: true, reloadOnChange: true)
    .AddJsonFile(String.Format("{0}/tinyidp.key", Environment.GetEnvironmentVariable("TINYIDP_SECU__PATH")),  optional: true, reloadOnChange: true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("UserInfoPolicy", o =>
    {
        o.AllowAnyOrigin();
        o.AllowAnyHeader();
        o.AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ApiPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 600, // 10 requêtes
                Window = TimeSpan.FromMinutes(1), // par minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
  {
    c.ResolveConflictingActions (apiDescriptions => apiDescriptions.First ());
  });

// Add services to the container.
builder.Services.AddResponseCompression();
builder.Services.AddHealthChecks();

builder.Services.AddDbContextPool<TinyidpContext>((serviceProvider, options) =>
{
    BddConfig? conf = builder.Configuration?.GetSection("TINYIDP_BDDCONFIG").Get<BddConfig>();

    if (conf == null)
        throw new Exception("No BDD configuration found");

    string connectString = string.Format("Host={0};Database={1};Username={2};Password={3}",
        conf.ServerName,
        conf.BddName,
        conf.UserName,
        conf.Password
        );
    options.UseNpgsql(connectString);
    options.LogTo(
        Console.WriteLine,  // Ou utilise un ILogger
        new[] { DbLoggerCategory.Database.Command.Name },
        LogLevel.Information,
        DbContextLoggerOptions.SingleLine | DbContextLoggerOptions.UtcTime
    );
#if DEBUG
    options.EnableSensitiveDataLogging();
#endif
    }
);

//builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// Infrastructure
builder.Services.AddScoped<ICredentialRepository, CredentialRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IKidRepository, KidRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddSingleton<ICacheTinyidp<Credential>, MemoryCacheTinyidp<Credential>>();
builder.Services.AddScoped<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<BackgroundSaveDB>();
builder.Services.AddSingleton<IQueueSaveDB<Credential>>(sp => 
    sp.GetRequiredService<BackgroundSaveDB>());
builder.Services.AddHostedService(sp => 
    sp.GetRequiredService<BackgroundSaveDB>());

//Business
builder.Services.AddScoped<ICredentialBusiness, CredentialBusiness>();
builder.Services.AddScoped<ITrustStoreService, TrustStoreService>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

// Secu
builder.Services.AddSingleton<IEncryptionService, RandomIvEncryptionService>();
builder.Services.AddScoped<IKeysManagment, KeysManagment>();
builder.Services.AddScoped<IPKCEService, PKCEService>();
builder.Services.AddScoped<IHashedPasswordPbkbf2, HashedPasswordPbkbf2>();

// Tokens
builder.Services.AddTokenStrategies();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });

//  Config kestrel
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
        options.AllowAnyClientCertificate();
    });

});

builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserInfoPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireClaim("Role", new[] { RoleCredential.Admin.ToString(), RoleCredential.User.ToString() });
    });
});

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

Console.WriteLine($"=== ENVIRONMENT: {app.Environment.EnvironmentName} ===");
Console.WriteLine($"=== IS DEVELOPMENT: {app.Environment.IsDevelopment()} ===");

app.UseRateLimiter();
var csp = app.Environment.IsDevelopment()
    ? "default-src 'self' https://cdn.jsdelivr.net; connect-src 'self' ws://localhost:*"
    : "default-src 'self' https://cdn.jsdelivr.net";
app.Use((context, next) =>
{
        context.Response.Headers["X-Frame-Options"] = "DENY";

        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        context.Response.Headers["X-Xss-Protection"] = "1; mode=block";

        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        context.Response.Headers["Referrer-Policy"] = "no-referrer";

        context.Response.Headers["Permissions-Policy"] = "camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";

        context.Response.Headers["Content-Security-Policy"] = csp;
        
        return next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapPost("/account/login-handler", async (
    HttpContext context,
    ICredentialBusiness credentialBusiness,
    [FromForm] string login,
    [FromForm] string password,
    [FromForm] string? returnUrl) =>
{
    if (await credentialBusiness.VerifyPassword(login, password))
    {
        var user = await credentialBusiness.GetCredentialBusinessEntityByIdent(login);

        if (user == null || (user.RoleIdent != RoleCredential.Admin && user.RoleIdent != RoleCredential.User))
            return Results.Redirect("/?error=unauthorized");

        user.LastIdent = DateTime.Now;
        await credentialBusiness.Update(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Ident),
            new Claim("FullName", user.Ident),
            new Claim("Role", user.RoleIdent.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    return Results.Redirect("/account/login?error=invalid");
}).RequireRateLimiting("ApiPolicy").DisableAntiforgery(); // ou gérez l'antiforgery manuellement

app.MapPost("/account/ident-handler", async (
    HttpContext context,
    ICredentialBusiness credentialBusiness,
    [FromForm] string login,
    [FromForm] string password,
    [FromForm] string? redirect_uri,
    [FromForm] string? scope,
    [FromForm] string? state,
    [FromForm] string? client_id,
    [FromForm] string? client_challenge,
    [FromForm] string? client_challenge_method,
    [FromForm] string? nonce)  =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Results.Redirect(HttpUtility.UrlEncode("/?error=L'identifiant et le mot de passe sont obligatoires."));
        }

        if (!(await credentialBusiness.VerifyPassword(login, password)))
        {
            return Results.Redirect(HttpUtility.UrlEncode("/?error=Identifiant ou mot de passe invalide."));
        }

        CredentialBusinessEntity? user = await credentialBusiness.GetCredentialBusinessEntityByIdent(login);
        if (user == null)
        {
            return Results.Redirect(HttpUtility.UrlEncode("/?error=L'utilisateur n'existe pas."));
        }

        credentialBusiness.CreateIdentityCooky(user, context);

        string url = string.Format("/oauth/authorize?response_type=code&redirect_uri={0}&scope={1}&state={2}&client_id={3}&code_challenge={4}&code_challenge_method={5}&nonce={6}",
            HttpUtility.UrlEncode(redirect_uri), scope, state, client_id, client_challenge, client_challenge_method, nonce ?? "");

        //context.Response.Redirect(url);
        return Results.Redirect(url);
    }
    catch (Exception ex)
    {
        var errorMessage = $"Erreur : {ex.Message}";
        return Results.Redirect(HttpUtility.UrlEncode($"/?error={errorMessage}"));
    }
}).RequireRateLimiting("ApiPolicy").DisableAntiforgery(); // ou gérez l'antiforgery manuellement

app.MapGet("/oauth/.well-known/openid-configuration", DiscoveryController.GetConfiguration).WithName("WellKnown")
    .Produces<DiscoveryResponse>(StatusCodes.Status200OK).RequireRateLimiting("ApiPolicy");
app.MapGet("/oauth/keys/jwks.json", KeysController.Jwks).WithName("Jwks")
    .Produces<KeysResponse>(StatusCodes.Status200OK).RequireRateLimiting("ApiPolicy");
app.MapPost("/oauth/token", 
    (   [FromForm] TokenRequest request, 
        ITokenService tokenService, 
        IHttpContextAccessor context) => 
    OAuthController.GetToken(
        request, 
        tokenService, 
        context))
    .WithName("GetToken")
    .DisableAntiforgery()
    //.Accepts<TokenRequestBusiness>("multipart/form-data")
    .Produces<TokenResponseBusiness>(StatusCodes.Status200OK)
    .RequireRateLimiting("ApiPolicy");
app.MapGet("/oauth/authorize", 
    (   string client_id, 
        string redirect_uri, 
        string response_type, 
        string? scope, 
        string? state, 
        string? nonce, 
        string? code_challenge, 
        string? code_challenge_method, 
        IHttpContextAccessor context, 
        ICredentialBusiness credentialBusiness) => 
    OAuthController.Authorize(
        response_type, 
        client_id, 
        redirect_uri, 
        scope, 
        state, 
        nonce, 
        code_challenge, 
        code_challenge_method, 
        context, 
        credentialBusiness))
    .WithName("Authorize")
    .RequireRateLimiting("ApiPolicy");
app.MapGet("/oauth/userinfo", (HttpContext context) => OAuthController.UserInfo).WithName("UserInfo")
    .Produces<tinyidp.Business.BusinessEntities.AppUser>(StatusCodes.Status200OK)
    .RequireAuthorization()
    .RequireRateLimiting("ApiPolicy");
app.MapGet("/pkce/challenge", (string code_verifier, IPKCEService pkceService) => PkceController.GenerateChallenge(pkceService, code_verifier )).WithName("PKCEChallenge")
    .RequireRateLimiting("ApiPolicy");
app.MapGet("/pkce/challenge/verifier", (int? size, IPKCEService pkceService) => PkceController.GenerateVerifierCode(pkceService, size )).WithName("PKCEVerifier")
    .RequireRateLimiting("ApiPolicy");
app.MapHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseResponseCompression();

app.UseBasicAuthHttpMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<tinyidp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
