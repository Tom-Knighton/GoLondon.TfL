using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Models;

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
        var response = await _client.GetAsync($"StopPoint/{id}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<tfl_StopPoint>(result);
    }

    public async Task<List<tfl_StopPoint>> GetByRadius(float lat, float lon, string? lineModeQuery, float radius, CancellationToken ct)
    {
        var url = $"StopPoint/?lat={lat}&lon={lon}&radius={radius}&stoptypes=NaptanMetroStation,NaptanRailStation,NaptanBusCoachStation,NaptanFerryPort,NaptanPublicBusCoachTram";
        if (!string.IsNullOrEmpty(lineModeQuery))
        {
            url += $"&modes={lineModeQuery}";
        }
        var response = await _client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<tfl_SearchResponse>(result);
        return searchResponse?.stopPoints;
    }
}