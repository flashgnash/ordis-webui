using Microsoft.Extensions.Options;

public class RollService {
    private readonly HttpClient _http;

    public RollService(HttpClient httpClient, IOptions<Dictionary<string,APIConfig>> apiConfigs) {
		_http = httpClient;
	}

	public async Task<RollResult> RollFor(PlayerCharacter character, String rollFormula) {


		var response = await _http.PostAsync($"http://localhost:3000/roll/{character.Id}/{rollFormula}",null);

		switch (response.StatusCode) {
			case System.Net.HttpStatusCode.InternalServerError:
			case System.Net.HttpStatusCode.BadRequest:
				throw new InvalidRollException();
			

			
		}


		return await response.Content.ReadFromJsonAsync<RollResult>();

	
	}

	
}
