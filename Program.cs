using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Ordis.Components;

// TEST-ONLY auth bypass gate. `--testing` is the ONLY way to enable it — there is
// deliberately no env-var/config fallback, so it can never be on in production.
// See TestingSupport.cs for the full explanation.
var testingMode = TestingAuth.IsEnabled(args);

// Strip the flag before it reaches the command-line configuration provider (which
// would otherwise reject a value-less switch), so it can only be read as a raw
// argument via TestingAuth.IsEnabled above.
var builder = WebApplication.CreateBuilder(
    args.Where(a => a != TestingAuth.TestingFlag).ToArray());

if (testingMode)
{
    Console.WriteLine(
        "[TESTING] --testing flag set: auto-authenticating as auto-generated 3-digit " +
        "test accounts. Do NOT use this in production.");
}

builder.Configuration.AddEnvironmentVariables();
Env.Load();

builder.Services.Configure<RpgConfig>(builder.Configuration.GetSection("RpgConfig"));
builder.Services.Configure<Dictionary<string, APIConfig>>(
    builder.Configuration.GetSection("ApiConfig")
);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<DiscordService>();
builder.Services.AddSingleton<LiveUpdateService>();

builder.Services.AddDbContextFactory<OrdisContext>((sp, opts) =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("CharacterDb"));
});

builder.Services.AddScoped<PlayerCharacterService>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddScoped<BuffService>();
builder.Services.AddScoped<UserState>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/* ---------------- AUTH ---------------- */

var discordConfig = builder.Configuration.GetSection("Discord");
var clientId = discordConfig["ClientId"]!;
var clientSecret = discordConfig["ClientSecret"]!;

// Normal Discord OAuth in production. When --testing is set, this also registers
// the auto-authenticating test scheme and makes it the default (see TestingSupport.cs).
TestingAuth.ConfigureAuthentication(builder.Services, testingMode, clientId, clientSecret);

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

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/");
});

if (testingMode)
{
    // TEST-ONLY: /testlogin lets browser tests switch between (or auto-create)
    // 3-digit test accounts. Only mapped under --testing (see TestingSupport.cs).
    TestingAuth.MapTestingEndpoints(app);
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdisContext>();
    db.Database.Migrate();
}

if (testingMode)
{
    // TEST-ONLY: seed the default test accounts + a usable character for each so
    // the roll panel is reachable for automated browser testing.
    await TestingAuth.SeedTestDataAsync(app.Services);
}

app.Run();
