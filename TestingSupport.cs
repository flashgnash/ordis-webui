using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// ---------------------------------------------------------------------------
// TEST-ONLY authentication bypass.
//
// Everything in this file is inert unless the app is started with the explicit
// `--testing` command-line flag (see TestingAuth.IsEnabled). There is NO
// environment-variable or configuration fallback: the bypass is fail-closed and
// can only be turned on by passing `--testing` on the command line, so it can
// never be accidentally enabled in production. Its sole purpose is to let
// automated browser tests (Playwright) reach the OW3N UI — notably the roll
// panel — without going through real Discord OAuth.
//
// Refinement over the original single-account bypass: `--testing` now supports
// ANY NUMBER of test accounts. Each test account is identified by an
// AUTO-GENERATED INTEGER id capped at 3 digits (0-999). Real Discord snowflakes
// are 17-19 digits, so a 3-digit test id can NEVER collide with — or be mistaken
// for — a real Discord user id. Anything with 4+ digits (including a real
// snowflake) is rejected outright: a test account can never assume a real id.
// ---------------------------------------------------------------------------

/// <summary>
/// Pure helpers describing the TEST-ONLY account id space. Test account ids are
/// integers strictly within <see cref="MinId"/>..<see cref="MaxId"/> (0-999, i.e.
/// at most 3 digits) so they can never collide with real 17-19 digit Discord
/// snowflakes. These helpers have no side effects and are safe to unit test in
/// isolation.
/// </summary>
public static class TestAccount
{
    /// <summary>Lowest permitted test account id.</summary>
    public const int MinId = 0;

    /// <summary>
    /// Highest permitted test account id. 999 is the largest 3-digit integer;
    /// the cap is what guarantees a test id can never reach real-snowflake width.
    /// </summary>
    public const int MaxId = 999;

    /// <summary>
    /// Account used when a request in `--testing` mode selects no specific
    /// account, so simply hitting the app authenticates as a valid test user.
    /// </summary>
    public const int DefaultId = 1;

    /// <summary>
    /// Test accounts seeded at startup so multiple distinct accounts (and their
    /// characters) exist out of the box for browser testing.
    /// </summary>
    public static readonly int[] DefaultSeedIds = { 1, 2, 3 };

    /// <summary>
    /// Request key (query-string parameter and cookie name) the test scheme reads
    /// to decide which test account to authenticate as.
    /// </summary>
    public const string SelectorKey = "testUser";

    /// <summary>Cookie set by <c>/testlogin</c> so a browser stays signed in as a chosen account.</summary>
    public const string CookieName = "ow3n_test_user";

    /// <summary>True only for integer ids inside the 3-digit (0-999) test range.</summary>
    public static bool IsValidId(int id) => id >= MinId && id <= MaxId;

    /// <summary>
    /// Parses a raw account-id string and ENFORCES the 3-digit (0-999) cap.
    /// Rejects null/blank input, non-numeric input, negatives, values with a
    /// leading sign or whitespace, and anything with 4 or more digits — which
    /// notably includes every real 17-19 digit Discord snowflake. Returns false
    /// (and <paramref name="id"/> = 0) on rejection.
    /// </summary>
    public static bool TryParseId(string? raw, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();

        // Reject by width first, so a 17-19 digit snowflake can never even be
        // parsed into range. "1000" and longer are rejected here too.
        if (raw.Length > 3)
            return false;

        // NumberStyles.None disallows signs and whitespace, so "-5" / "+5" / " 5 "
        // are rejected rather than silently coerced.
        if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            return false;

        if (!IsValidId(parsed))
            return false;

        id = parsed;
        return true;
    }

    /// <summary>
    /// Returns the smallest unused test id in range given the ids already in use,
    /// or null if the entire 0-999 space is exhausted. Used to AUTO-GENERATE ids
    /// for new test accounts.
    /// </summary>
    public static int? NextFreeId(ISet<int> used)
    {
        for (var candidate = MinId; candidate <= MaxId; candidate++)
        {
            if (!used.Contains(candidate))
                return candidate;
        }
        return null;
    }

    /// <summary>Stable string form of a test id, used as the User primary key / discord_id claim.</summary>
    public static string ToUserId(int id) => id.ToString(CultureInfo.InvariantCulture);

    /// <summary>Human-readable display name for a test account.</summary>
    public static string ToDisplayName(int id) => $"Test Account {id}";
}

/// <summary>
/// Authentication handler used ONLY in `--testing` mode. It authenticates each
/// request as a test account selected by the <c>testUser</c> query parameter or
/// the <c>ow3n_test_user</c> cookie (falling back to the default account when
/// neither is present). The selected id is validated through
/// <see cref="TestAccount.TryParseId"/>, so a 4+ digit / real-style id is
/// rejected instead of being honoured. It is only ever registered when
/// <see cref="TestingAuth.ConfigureAuthentication"/> wires it up, which itself
/// only happens under the `--testing` flag.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>Name of the test authentication scheme.</summary>
    public const string SchemeName = "Testing";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Pick the account: explicit ?testUser=N wins, then the cookie set by
        // /testlogin, then the default account so a bare request still works.
        var raw = Request.Query[TestAccount.SelectorKey].FirstOrDefault()
                  ?? Request.Cookies[TestAccount.CookieName];

        int id;
        if (raw is null)
        {
            id = TestAccount.DefaultId;
        }
        else if (!TestAccount.TryParseId(raw, out id))
        {
            // A 4+ digit / real-style id (or any out-of-range / non-numeric value)
            // is rejected — a test account must never be able to assume a real
            // Discord id. Failing here means the [Authorize] pipeline challenges
            // (HTTP 401) rather than authenticating.
            return Task.FromResult(AuthenticateResult.Fail(
                $"Invalid test account id '{raw}': must be an integer " +
                $"{TestAccount.MinId}-{TestAccount.MaxId} (real Discord ids are not permitted for test accounts)."));
        }

        // Mint the same claims the real Discord OAuth flow produces, so every
        // downstream check (the "HasDiscordId" policy, AuthenticationStateProvider,
        // pages reading the discord_id/discord_name claims) behaves normally.
        var claims = new[]
        {
            new Claim("discord_id", TestAccount.ToUserId(id)),
            new Claim("discord_name", TestAccount.ToDisplayName(id)),
            new Claim(ClaimTypes.Name, TestAccount.ToDisplayName(id)),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Helpers that centralise how authentication is configured and, when in
/// `--testing` mode, how test accounts and their characters are seeded and
/// switched.
/// </summary>
public static class TestingAuth
{
    /// <summary>The one and only switch that enables the test bypass.</summary>
    public const string TestingFlag = "--testing";

    /// <summary>
    /// Returns true ONLY when the explicit <c>--testing</c> flag is present in
    /// the process command-line arguments. Deliberately does not consult
    /// environment variables or configuration — the bypass is fail-closed.
    /// </summary>
    public static bool IsEnabled(string[] args) => args.Contains(TestingFlag);

    /// <summary>
    /// Configures authentication for the app. When <paramref name="testingMode"/>
    /// is false this is exactly the normal Discord OAuth setup. When true, a test
    /// scheme is added and made the default so requests are auto-authenticated as
    /// a (validated, 3-digit) test account; normal Discord auth is left registered
    /// but unused.
    /// </summary>
    public static void ConfigureAuthentication(
        IServiceCollection services,
        bool testingMode,
        string clientId,
        string clientSecret)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            if (testingMode)
            {
                // TEST-ONLY: make the auto-authenticating test scheme the default,
                // so [Authorize] pages are reachable without real Discord login.
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }
            else
            {
                // Normal, production behaviour — unchanged.
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Discord";
            }
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

        if (testingMode)
        {
            // Only ever registered under --testing.
            authBuilder.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, displayName: null, configureOptions: _ => { });
        }
    }

    /// <summary>
    /// Seeds the default set of test accounts (and a usable character for each) so
    /// multiple distinct 3-digit accounts exist for browser testing and the roll
    /// panel is reachable out of the box. Idempotent, and only ever called when
    /// running under <c>--testing</c>.
    /// </summary>
    public static async Task SeedTestDataAsync(IServiceProvider services)
    {
        foreach (var id in TestAccount.DefaultSeedIds)
            await EnsureAccountSeededAsync(services, id);
    }

    /// <summary>
    /// Ensures a single test account (a <see cref="User"/> plus one usable
    /// <see cref="PlayerCharacter"/>) exists for the given 3-digit id. Idempotent.
    /// Throws if the id is somehow out of range — a defensive backstop; callers
    /// validate via <see cref="TestAccount.TryParseId"/> first.
    /// </summary>
    public static async Task EnsureAccountSeededAsync(IServiceProvider services, int id)
    {
        if (!TestAccount.IsValidId(id))
            throw new ArgumentOutOfRangeException(nameof(id), id,
                $"Test account id must be {TestAccount.MinId}-{TestAccount.MaxId}.");

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdisContext>();

        var userId = TestAccount.ToUserId(id);

        if (await db.Users.FindAsync(userId) == null)
        {
            db.Users.Add(new User
            {
                Id = userId,
                Username = TestAccount.ToDisplayName(id),
            });
        }

        var hasCharacter = await db.Characters.AnyAsync(c => c.UserId == userId);
        if (!hasCharacter)
        {
            // A minimal but complete character: a stat block with stats and a
            // default roll so the RollMenu renders its stat buttons and the Roll
            // button submits "1d20" to the roll service out of the box.
            db.Characters.Add(new PlayerCharacter
            {
                UserId = userId,
                Name = $"Test Character {id}",
                Gauges = new List<Gauge>(),
                Inventory = new List<Item>(),
                Spells = new List<Spell>(),
                StatBlock = new StatBlock
                {
                    Level = 1,
                    MaxHealth = 20,
                    CurrentHealth = 20,
                    DefaultRoll = "1d20",
                    Stats = new List<Stat>
                    {
                        new() { Name = "str", Value = 12 },
                        new() { Name = "dex", Value = 14 },
                        new() { Name = "con", Value = 10 },
                    },
                },
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Maps the TEST-ONLY <c>/testlogin</c> endpoint (called ONLY under
    /// <c>--testing</c>). It lets a browser test switch between — or create — test
    /// accounts:
    /// <list type="bullet">
    ///   <item><c>/testlogin?testUser=N</c> signs in as the 3-digit account N.</item>
    ///   <item><c>/testlogin</c> with no id AUTO-GENERATES the next free id.</item>
    /// </list>
    /// The account (and its character) is seeded on demand, a persistent cookie is
    /// set so the browser stays signed in, and the request is redirected to
    /// <c>ReturnUrl</c> (or <c>/</c>). Any id that fails the 3-digit cap is
    /// rejected with HTTP 400 — a real Discord id can never be used.
    /// </summary>
    public static void MapTestingEndpoints(WebApplication app)
    {
        app.MapGet("/testlogin", async (HttpContext ctx) =>
        {
            var db = ctx.RequestServices.GetRequiredService<OrdisContext>();

            var requested = ctx.Request.Query[TestAccount.SelectorKey].FirstOrDefault();

            int id;
            if (string.IsNullOrWhiteSpace(requested))
            {
                // AUTO-GENERATE: pick the smallest free id across existing users
                // that are already 3-digit test ids.
                var used = (await db.Users.Select(u => u.Id).ToListAsync())
                    .Where(uid => TestAccount.TryParseId(uid, out _))
                    .Select(uid => { TestAccount.TryParseId(uid, out var n); return n; })
                    .ToHashSet();

                var next = TestAccount.NextFreeId(used);
                if (next is null)
                {
                    ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                    await ctx.Response.WriteAsync("All test account ids (0-999) are in use.");
                    return;
                }
                id = next.Value;
            }
            else if (!TestAccount.TryParseId(requested, out id))
            {
                // Reject a 4+ digit / real-style id outright.
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                await ctx.Response.WriteAsync(
                    $"Invalid test account id '{requested}': must be an integer " +
                    $"{TestAccount.MinId}-{TestAccount.MaxId}. Real Discord ids are not permitted.");
                return;
            }

            await EnsureAccountSeededAsync(app.Services, id);

            ctx.Response.Cookies.Append(TestAccount.CookieName, TestAccount.ToUserId(id), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
            });

            var returnUrl = ctx.Request.Query["ReturnUrl"].ToString();
            ctx.Response.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
        });
    }
}
