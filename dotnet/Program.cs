using dotnet.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RpgConfig>(
    builder.Configuration.GetSection("RpgConfig"));



var connStr = builder.Configuration.GetConnectionString("CharacterDb");
builder.Services.AddSingleton(new PlayerCharacterService(connStr));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.Run();
