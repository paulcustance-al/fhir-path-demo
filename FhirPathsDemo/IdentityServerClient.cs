using IdentityModel.Client;

namespace FhirPathsDemo;

public static class IdentityServerClient
{
    private static readonly HttpClient IdentityServerHttpClient;

    static IdentityServerClient()
    {
        IdentityServerHttpClient = new HttpClient();
        IdentityServerHttpClient.BaseAddress = new Uri("https://ppmplusuat2idsrv.leedsth.nhs.uk");
        IdentityServerHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private static ClientCredentialsTokenRequest ClientCredentialsTokenRequest => new()
    {
        Address = "https://ppmplusuat2idsrv.leedsth.nhs.uk/connect/token",
        ClientId = "test-automation",
        ClientSecret = "e5420345-42d7-48fe-93d0-9c6c88afc203",
        Scope = "cds-fhir-api/read"
    };

    public static async Task<string?> GetEhrFhirApiToken()
    {
        var tokenResponse = await IdentityServerHttpClient
            .RequestClientCredentialsTokenAsync(ClientCredentialsTokenRequest);

        if (tokenResponse.AccessToken is null)
            throw new Exception("Couldn't retrieve an access token");

        return tokenResponse.AccessToken;
    }
}