using System.Security.Claims;
using System.Text.Json;
using dotnet.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;
var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();


builder.Services.Configure<RpgConfig>(
    builder.Configuration.GetSection("RpgConfig"));

builder.Services.Configure<Dictionary<string,APIConfig>>(
    builder.Configuration.GetSection("ApiConfig"));

Env.Load();

var connStr = builder.Configuration.GetConnectionString("CharacterDb");

builder.Services.AddHttpClient();

builder.Services.AddSingleton(new PlayerCharacterService(connStr));
builder.Services.AddSingleton<RollService>();
builder.Services.AddSingleton<DiscordService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie();
builder.Services.AddAuthorization(
    options => {
        options.AddPolicy("HasDiscordId", policy =>
            policy.RequireClaim("discord_id"));
    }
    
);

builder.WebHost.ConfigureKestrel((context, options) => {
    options.Configure(context.Configuration.GetSection("Kestrel"));
});   
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


var discordConfig = builder.Configuration.GetSection("Discord");
var clientId = discordConfig["ClientId"];
var clientSecret = discordConfig["ClientSecret"];


var baseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

// Should move this somewhere else - currently unsure of how to implement API endpoints in a blazor pages setup
// At the very least it should be made into a separate function in another file and called from here
app.MapGet("/auth/callback", async (HttpContext http) =>
{
    var query = http.Request.Query;
    if (!query.TryGetValue("code", out var code))
        return Results.BadRequest("No code in query string");

    var discordCode = code.ToString();

    var values = new Dictionary<string,string>{
        {"client_id",clientId},
        {"client_secret",clientSecret},
        {"grant_type","authorization_code"},
        {"code", discordCode},
        {"redirect_uri",$"{baseUrl}/auth/callback"}
    };

    var client = new HttpClient();
    var response = await client.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        return Results.Problem($"Discord token endpoint returned {response.StatusCode}: {content}");

    JsonElement token;
    try
    {
        token = JsonSerializer.Deserialize<JsonElement>(content);
    }
    catch
    {
        return Results.Problem($"Failed to parse Discord token response: {content}");
    }

    if (!token.TryGetProperty("access_token", out var accessTokenProp))
        return Results.Problem($"access_token not found in response: {content}");

    var access = accessTokenProp.GetString();

    // get user info
    var request = new HttpRequestMessage(HttpMethod.Get,"https://discord.com/api/users/@me");
    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
    var userResponse = await client.SendAsync(request);
    var userContent = await userResponse.Content.ReadAsStringAsync();

    if (!userResponse.IsSuccessStatusCode)
        return Results.Problem($"Discord /users/@me returned {userResponse.StatusCode}: {userContent}");

    var userJson = JsonSerializer.Deserialize<JsonElement>(userContent);
    var discordId = userJson.GetProperty("id").GetString();
    var discordName = userJson.GetProperty("username").GetString();

    // sign in
    var claims = new List<Claim>{ new Claim("discord_id", discordId), new Claim("discord_name",discordName) };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect("/");

});

app.Run();
