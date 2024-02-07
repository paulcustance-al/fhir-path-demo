using System.Net.Http.Headers;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Task = System.Threading.Tasks.Task;

namespace FhirPathsDemo;

public class EhrFhirApiClient
{
    private HttpClient? _httpClient;

    public async Task<ISourceNode> GetExtendedCondition(string rootInstanceId, string patientId)
    {
        await Initialise();

        var url = $"/R3/extended/Condition/{rootInstanceId}?patientIdentifier={patientId}";
        var responseBody = await _httpClient!.GetStringAsync(url);

        return await FhirJsonNode.ParseAsync(responseBody);
    }
    
    private async Task Initialise()
    {
        if (_httpClient is not null) return;

        var accessToken = await IdentityServerClient.GetEhrFhirApiToken();

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://cds-fhir-api-uat2.leedsth.nhs.uk");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}