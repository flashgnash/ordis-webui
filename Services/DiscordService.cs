using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class DiscordService(IConfiguration config, HttpClient http)
{

	// This is very bad but is only a temporary measure to get up and running.
	// Still undecided on how best to get the webhook URI
	// Also, might not bother with this method at all ikn the long run and just have the bot send the
	// message with CachedHttp, but rust is being a pain so I'd like to avoid that for now
    public async Task SendMessageAsync(string message)
    {
        var payload = JsonSerializer.Serialize(new { content = message });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await http.PostAsync(config["DiscordWebhookUrl"], content);
    }


	private string ColorHexFromString(string input)
	{
	    using var md5 = MD5.Create();
	    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
	    return $"#{hash[0]:X2}{hash[1]:X2}{hash[2]:X2}";
	}


    public async Task SendEmbedAsync(string title, string description, string? colorHex = null)
    {

		colorHex = colorHex ?? ColorHexFromString(title);
        var embed = new
        {
            title,
            description,
            color = int.Parse(colorHex.Replace("#", ""), System.Globalization.NumberStyles.HexNumber)
        };

        var payload = new { embeds = new[] { embed } };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await http.PostAsync(config["DiscordWebhookUrl"], content);
    }
	
}
