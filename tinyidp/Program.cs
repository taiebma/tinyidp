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
builder.Services.AddControllers().AddNewtonsoftJson();
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

builder.Services.AddDbContext<TinyidpContext>(options =>
{
//    options.EnableSensitiveDataLogging();
}
);

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// Repo
builder.Services.AddScoped<ICredentialRepository, CredentialRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IKidRepository, KidRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

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

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseBasicAuthHttpMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
