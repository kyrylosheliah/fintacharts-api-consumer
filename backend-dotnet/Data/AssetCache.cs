using System.Net.Http.Headers;
using System.Text.Json;
using BackendDotnet.Models;
using BackendDotnet.Services;

namespace BackendDotnet.Data;

public class AssetCache
{
    public readonly Dictionary<string, Instrument> instruments = [];
    public readonly Dictionary<string, List<string>> exchanges = [];
    public readonly List<string> providers = [];

    private readonly Lock _lock = new();

    private readonly AuthTokenService _tokenService;
    private readonly HttpClient _httpClient;

    private readonly string _baseUri;

    public AssetCache(
        AuthTokenService tokenService,
        IConfiguration configuration,
        HttpClient httpClient
    )
    {
        _tokenService = tokenService;
        _httpClient = httpClient;
        _baseUri = configuration["Base:Url"] ?? "";
        _ = LoadProviders();
        _ = LoadExchanges();
    }

    private async Task LoadProviders()
    {
        var bearerToken = await _tokenService.GetAccessTokenAsync();
        if (bearerToken == null)
            return;
        var url = $"{_baseUri}/api/instruments/v1/providers";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStreamAsync();
            var providerResponse = await JsonSerializer.DeserializeAsync<ProviderResponse>(responseString);
            if (providerResponse != null)
                UpdateProviders(providerResponse.Data);
        }
    }

    private async Task LoadExchanges()
    {
        var bearerToken = await _tokenService.GetAccessTokenAsync();
        if (bearerToken == null)
            return;
        var url = $"{_baseUri}/api/instruments/v1/exchanges";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStreamAsync();
            var exchangeResponse = await JsonSerializer.DeserializeAsync<ExchangeResponse>(responseString);
            if (exchangeResponse != null)
                UpdateExchanges(exchangeResponse.Data);
        }
    }

    public void UpdateInstruments(List<Instrument> data)
    {
        lock (_lock)
        {
            foreach (var instrument in data)
            {
                if (!instruments.ContainsKey(instrument.Id))
                {
                    instruments[instrument.Symbol] = instrument;
                }
            }
        }
    }

    public void UpdateExchanges(Dictionary<string, List<string>> data)
    {
        lock (_lock)
        {
            foreach (var key in data.Keys)
            {
                if (!exchanges.ContainsKey(key))
                    exchanges[key] = [];
                foreach (var item in data[key])
                    if (!exchanges[key].Contains(item))
                        exchanges[key].Add(item);
            }
        }
    }

    public void UpdateProviders(List<string> data)
    {
        lock (_lock)
        {
            foreach (var provider in data)
                if (!providers.Contains(provider))
                    providers.Add(provider);
        }
    }
}