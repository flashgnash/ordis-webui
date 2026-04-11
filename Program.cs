using System.Security.Claims;
using System.Text.Json;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Ordis.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
Env.Load();

builder.Services.Configure<RpgConfig>(builder.Configuration.GetSection("RpgConfig"));
builder.Services.Configure<Dictionary<string, APIConfig>>(
    builder.Configuration.GetSection("ApiConfig")
);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<DiscordService>();

builder.Services.AddDbContextFactory<OrdisContext>((sp, opts) =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("CharacterDb"));
});

builder.Services.AddScoped<PlayerCharacterService>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddScoped<UserState>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/* ---------------- AUTH ---------------- */

var discordConfig = builder.Configuration.GetSection("Discord");
var clientId = discordConfig["ClientId"]!;
var clientSecret = discordConfig["ClientSecret"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
})

.AddCookie(o =>
{
    o.LoginPath = "/login";
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOAuth("Discord", options =>
{
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;

    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

    options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
    options.TokenEndpoint = "https://discord.com/api/oauth2/token";
    options.UserInformationEndpoint = "https://discord.com/api/users/@me";

    options.CallbackPath = "/signin-discord";
    options.Scope.Add("identify");
    options.SaveTokens = true;

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async ctx =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
            var res = await ctx.Backchannel.SendAsync(req);
            var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());

            var discordId = json.RootElement.GetProperty("id").GetString()!;
            var discordName = json.RootElement.GetProperty("username").GetString()!;

            ctx.Identity!.AddClaim(new Claim("discord_id", discordId));
            ctx.Identity.AddClaim(new Claim("discord_name", discordName));

            // Add to DB if not exist
            using var scope = ctx.HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdisContext>();

            var user = await db.Users.FindAsync(discordId);
            if (user == null)
            {
                user = new User
                {
                    Id = discordId,
                    Username = discordName
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasDiscordId", p => p.RequireClaim("discord_id"));
});

/* ---------------- APP ---------------- */

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

/* login endpoint — preserves ReturnUrl */
app.MapGet("/login", async (HttpContext ctx) =>
{
    var returnUrl = ctx.Request.Query["ReturnUrl"].ToString();
    await ctx.ChallengeAsync("Discord", new AuthenticationProperties
    {
        RedirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl
    });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
