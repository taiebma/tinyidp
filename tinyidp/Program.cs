using tinyidp.Business.Credential;
using tinyidp.infrastructure.bdd;
using tinyidp.Encryption;
using Microsoft.AspNetCore.Authentication.Cookies;
using tinyidp.infrastructure.keysmanagment;
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

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/var/cache/secrets/tinyidp.db", optional: true, reloadOnChange: true)
    .AddJsonFile(String.Format("{0}/tinyidp.key", Environment.GetEnvironmentVariable("TINYIDP_SECU__PATH")),  optional: true, reloadOnChange: true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("UserInfoPolicy", o =>
    {
        o.AllowAnyOrigin();
        o.AllowAnyHeader();
        o.AllowAnyMethod();
    });
});
builder.Services.AddSession();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
  {
    c.ResolveConflictingActions (apiDescriptions => apiDescriptions.First ());
  });

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Credentials");
    options.Conventions.AuthorizeFolder("/Certificates");
    options.Conventions.AuthorizeFolder("/ThrustStore");
    options.Conventions.AuthorizeFolder("/Kids");
    options.Conventions.AuthorizeFolder("/Token");
    options.Conventions.AllowAnonymousToFolder("/Login");
});

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
    options.EnableSensitiveDataLogging();
}
);

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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
builder.Services.AddScoped<IThrustStoreService, ThrustStoreService>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

// Secu
builder.Services.AddSingleton<IEncryptionService, RandomIvEncryptionService>();
builder.Services.AddScoped<IKeysManagment, KeysManagment>();
builder.Services.AddScoped<IPKCEService, PKCEService>();
builder.Services.AddScoped<IHashedPasswordPbkbf2, HashedPasswordPbkbf2>();

// Tokens
builder.Services.AddTokenStrategies();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

//  Config kestrel
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
    {
        options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
        options.AllowAnyClientCertificate();
    });

});

builder.Services.AddOpenApi();

var app = builder.Build();

Console.WriteLine($"=== ENVIRONMENT: {app.Environment.EnvironmentName} ===");
Console.WriteLine($"=== IS DEVELOPMENT: {app.Environment.IsDevelopment()} ===");

app.Use((context, next) =>
{
        context.Response.Headers["X-Frame-Options"] = "DENY";

        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        context.Response.Headers["X-Xss-Protection"] = "1; mode=block";

        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        context.Response.Headers["Referrer-Policy"] = "no-referrer";

        context.Response.Headers["Permissions-Policy"] = "camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()";

        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'";
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapGet("/oauth/.well-known/openid-configuration", DiscoveryController.GetConfiguration).WithName("WellKnown");
app.MapGet("/oauth/keys/jwks.json", KeysController.Jwks).WithName("Jwks");
app.MapPost("/oauth/token", OAuthController.GetToken).WithName("GetToken").DisableAntiforgery();;
app.MapGet("/oauth/authorize", OAuthController.Authorize).WithName("Authorize");
app.MapGet("/oauth/userinfo", OAuthController.UserInfo).WithName("UserInfo");
app.MapHealthChecks("/health");

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseResponseCompression();

app.UseBasicAuthHttpMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();

//app.MapControllers();

app.Run();
