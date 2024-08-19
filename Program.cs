using tinyidp.Business.Credential;
using tinyidp.infrastructure.bdd;
using tinyidp.Encryption;
using Microsoft.AspNetCore.Authentication.Cookies;
using tinyidp.infrastructure.keysmanagment;
using tinyidp.WebAuthent.Modules;
using tinyidp.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
builder.Services.AddControllers();
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
    options.Conventions.AuthorizeFolder("/Kids");
    options.Conventions.AuthorizeFolder("/Token");
    options.Conventions.AllowAnonymousToFolder("/Login");
});

builder.Services.AddDbContext<TinyidpContext>(options =>
{
    options.EnableSensitiveDataLogging();
}
);

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// Repo
builder.Services.AddScoped<ICredentialRepository, CredentialRepository>();
builder.Services.AddScoped<IKidRepository, KidRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

//Business
builder.Services.AddScoped<ICredentialBusiness, CredentialBusiness>();

// Secu
builder.Services.AddSingleton<IEncryptionService, RandomIvEncryptionService>();
builder.Services.AddScoped<IKeysManagment, KeysManagment>();
builder.Services.AddScoped<IPKCEService, PKCEService>();

// Tokens
builder.Services.AddTokenStrategies();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

var app = builder.Build();
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
