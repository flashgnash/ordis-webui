using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ordis.Tests;

/// <summary>
/// Verifies the TEST-ONLY `--testing` auth bypass and its 3-digit account cap:
///   * with the flag on, the test scheme auto-authenticates ANY NUMBER of
///     accounts, each identified by a distinct 3-digit integer id;
///   * a 4+ digit / real-style id is rejected (never honoured as a test account);
///   * with the flag off, protected routes still redirect to Discord login.
/// </summary>
public class AuthBypassTests
{
    // Spins up an in-memory app wired with the exact authentication configuration
    // production uses (via TestingAuth.ConfigureAuthentication), plus one protected
    // endpoint standing in for OW3N's [Authorize(Policy = "HasDiscordId")] pages.
    private static async Task<(IHost host, HttpClient client)> BuildAsync(bool testingMode)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        TestingAuth.ConfigureAuthentication(
            builder.Services, testingMode, "dummy-client-id", "dummy-client-secret");

        builder.Services.AddAuthorization(options =>
            options.AddPolicy("HasDiscordId", p => p.RequireClaim("discord_id")));
        builder.Services.AddRouting();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", (ClaimsPrincipal user) =>
                "discord_id=" + user.FindFirst("discord_id")?.Value)
            .RequireAuthorization("HasDiscordId");

        await app.StartAsync();
        return (app, app.GetTestClient());
    }

    [Fact]
    public async Task WithTestingFlag_NoSelector_AuthenticatesDefaultAccount()
    {
        var (host, client) = await BuildAsync(testingMode: true);
        using var _ = host;

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal($"discord_id={TestAccount.DefaultId}", body);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("7")]
    [InlineData("42")]
    [InlineData("999")]
    public async Task WithTestingFlag_EnablesMultipleAccounts_WithDistinctThreeDigitIds(string idText)
    {
        var (host, client) = await BuildAsync(testingMode: true);
        using var _ = host;

        // Each distinct 3-digit id authenticates as its own account.
        var response = await client.GetAsync($"/?{TestAccount.SelectorKey}={idText}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal($"discord_id={idText}", body);

        // The minted id really is inside the 3-digit (0-999) test range.
        Assert.True(int.TryParse(idText, out var id) && TestAccount.IsValidId(id));
    }

    [Fact]
    public async Task WithTestingFlag_DistinctSelectors_YieldDistinctAccounts()
    {
        var (host, client) = await BuildAsync(testingMode: true);
        using var _ = host;

        var first = await (await client.GetAsync($"/?{TestAccount.SelectorKey}=3")).Content.ReadAsStringAsync();
        var second = await (await client.GetAsync($"/?{TestAccount.SelectorKey}=8")).Content.ReadAsStringAsync();

        Assert.Equal("discord_id=3", first);
        Assert.Equal("discord_id=8", second);
        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData("1000")]                 // 4 digits — one past the cap
    [InlineData("123456789012345678")]   // real-style 18-digit Discord snowflake
    [InlineData("-1")]                   // negative
    [InlineData("abc")]                  // non-numeric
    public async Task WithTestingFlag_FourPlusDigitOrRealStyleId_IsRejected(string idText)
    {
        var (host, client) = await BuildAsync(testingMode: true);
        using var _ = host;

        var response = await client.GetAsync($"/?{TestAccount.SelectorKey}={idText}");

        // Rejected: authentication fails, so the protected route is not served.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithoutTestingFlag_ProtectedRouteRedirectsToDiscordLogin()
    {
        var (host, client) = await BuildAsync(testingMode: false);
        using var _ = host;

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(
            "discord.com/oauth2/authorize",
            response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--other" }, false)]
    [InlineData(new[] { "--testing" }, true)]
    [InlineData(new[] { "run", "--testing" }, true)]
    public void IsEnabled_OnlyTrueWhenTestingFlagPresent(string[] args, bool expected)
    {
        Assert.Equal(expected, TestingAuth.IsEnabled(args));
    }
}
