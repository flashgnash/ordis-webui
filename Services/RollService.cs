using Microsoft.Extensions.Options;

public class RollService {
    private readonly HttpClient _http;

    public RollService(HttpClient httpClient, IOptions<Dictionary<string,APIConfig>> apiConfigs) {
		_http = httpClient;
	}
	public async Task<RollResult> Roll(String rollFormula) {


		await _http.GetAsync("http://localhost:3000/roll/1");


		return new();
	}

	public async Task<RollResult> RollFor(PlayerCharacter character, String rollFormula) {


		var response = await _http.PostAsync($"http://localhost:3000/roll/{character.Id}/{rollFormula}",null);

		return await response.Content.ReadFromJsonAsync<RollResult>();

	
	}

	
}
