using CatalogoApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde .env si existe (evitando fallos si no existe)
var envPaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "CatalogoApi", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env")
};

foreach (var envPath in envPaths)
{
    if (File.Exists(envPath))
    {
        DotNetEnv.Env.Load(envPath);
        break;
    }
}

// Add services to the container.
// La API responde con DTOs; IgnoreCycles queda como red de seguridad para no
// volver a fallar con JsonException si alguna entidad se serializa por error.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "Data Source=catalogo.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Helper para obtener configuración con prioridad: variable de entorno -> IConfiguration -> dummy
string GetConfig(string envVar, string configPath, string fallback)
{
    var val = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(val)) return val;
    val = builder.Configuration[configPath];
    if (!string.IsNullOrWhiteSpace(val)) return val;
    return fallback;
}

// Configuración de autenticación con cookies y OAuth 2.0 (Twitch y X)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;

    // Responder 401 Unauthorized para peticiones no autenticadas a /api/* en vez de redirección 302
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
})
.AddTwitch("Twitch", options =>
{
    options.ClientId = GetConfig("TWITCH_CLIENT_ID", "Authentication:Twitch:ClientId", "dummy-twitch-client-id");
    options.ClientSecret = GetConfig("TWITCH_CLIENT_SECRET", "Authentication:Twitch:ClientSecret", "dummy-twitch-client-secret");
    options.CallbackPath = new PathString(GetConfig("TWITCH_CALLBACK_PATH", "Authentication:Twitch:CallbackPath", "/signin-twitch"));
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Twitch"));
        return Task.CompletedTask;
    };
})
.AddTwitter("Twitter", options =>
{
    options.ClientId = GetConfig("TWITTER_CLIENT_ID", "Authentication:X:ClientId", "dummy-twitter-client-id");
    options.ClientSecret = GetConfig("TWITTER_CLIENT_SECRET", "Authentication:X:ClientSecret", "dummy-twitter-client-secret");
    options.CallbackPath = new PathString(GetConfig("TWITTER_CALLBACK_PATH", "Authentication:X:CallbackPath", "/signin-twitter"));
    // OAuth 2.0 con endpoint de usuario básico sin requerir email
    options.UserInformationEndpoint = "https://api.twitter.com/2/users/me";
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Twitter"));
        return Task.CompletedTask;
    };
})
.AddDiscord("Discord", options =>
{
    options.ClientId = GetConfig("DISCORD_CLIENT_ID", "Authentication:Discord:ClientId", "dummy-discord-client-id");
    options.ClientSecret = GetConfig("DISCORD_CLIENT_SECRET", "Authentication:Discord:ClientSecret", "dummy-discord-client-secret");
    options.CallbackPath = new PathString(GetConfig("DISCORD_CALLBACK_PATH", "Authentication:Discord:CallbackPath", "/signin-discord"));
    options.Scope.Add("identify");
    options.Scope.Add("email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Discord"));
        return Task.CompletedTask;
    };
})
.AddSpotify("Spotify", options =>
{
    options.ClientId = GetConfig("SPOTIFY_CLIENT_ID", "Authentication:Spotify:ClientId", "dummy-spotify-client-id");
    options.ClientSecret = GetConfig("SPOTIFY_CLIENT_SECRET", "Authentication:Spotify:ClientSecret", "dummy-spotify-client-secret");
    options.CallbackPath = new PathString(GetConfig("SPOTIFY_CALLBACK_PATH", "Authentication:Spotify:CallbackPath", "/signin-spotify"));
    options.Scope.Add("user-read-email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Spotify"));
        return Task.CompletedTask;
    };
})
.AddDropbox("Dropbox", options =>
{
    options.ClientId = GetConfig("DROPBOX_CLIENT_ID", "Authentication:Dropbox:ClientId", "dummy-dropbox-client-id");
    options.ClientSecret = GetConfig("DROPBOX_CLIENT_SECRET", "Authentication:Dropbox:ClientSecret", "dummy-dropbox-client-secret");
    options.CallbackPath = new PathString(GetConfig("DROPBOX_CALLBACK_PATH", "Authentication:Dropbox:CallbackPath", "/signin-dropbox"));
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Dropbox"));
        return Task.CompletedTask;
    };
})
.AddSlack("Slack", options =>
{
    options.ClientId = GetConfig("SLACK_CLIENT_ID", "Authentication:Slack:ClientId", "dummy-slack-client-id");
    options.ClientSecret = GetConfig("SLACK_CLIENT_SECRET", "Authentication:Slack:ClientSecret", "dummy-slack-client-secret");
    options.CallbackPath = new PathString(GetConfig("SLACK_CALLBACK_PATH", "Authentication:Slack:CallbackPath", "/signin-slack"));
    options.Scope.Add("identity.basic");
    options.Scope.Add("identity.email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Slack"));
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
