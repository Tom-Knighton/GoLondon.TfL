using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.ServiceCollections.TfL;

public class TflApiClient : ITfLApiClient
{
    private readonly HttpClient _client;
    private const string ClientName = "TfLApiClient";

    public TflApiClient(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient(ClientName);
    }
    
    public async Task<tfl_StopPoint> GetStopPointByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetAsync($"StopPoint/{id}", ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<tfl_StopPoint>(result);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}