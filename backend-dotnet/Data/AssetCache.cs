using System.Net.Http.Headers;
using System.Text.Json;
using BackendDotnet.Models;
using BackendDotnet.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace BackendDotnet.Data;

public class AssetCache
{
    // public readonly Dictionary<string, Instrument> instruments = [];
    public readonly List<string> symbols = [];
    public readonly Dictionary<string, List<string>> exchanges = [];
    public readonly List<string> providers = [];

    private readonly Lock _lock = new();

    private readonly AuthTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private readonly string _baseUri;

    public AssetCache(
        AuthTokenService tokenService,
        IConfiguration configuration,
        HttpClient httpClient
    )
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _httpClient = httpClient;
        _baseUri = _configuration["Base:Uri"] ?? "";
        _ = LoadProviders();
        _ = LoadExchanges();
    }

    public async Task<List<Instrument>> GetInstruments(
        string? symbol,
        int? page,
        int? size,
        string? provider
    )
    {
        var bearerToken = await _tokenService.GetAccessTokenAsync();
        if (bearerToken == null)
            return [];
        string url = $"{_baseUri}/api/instruments/v1/instruments";
        var query = new Dictionary<string, string?>()
        {
            ["symbol"] = symbol,
            ["page"] = page?.ToString(),
            ["size"] = size?.ToString(),
            ["provider"] = provider
        };
        var uri = QueryHelpers.AddQueryString(url, query);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return [];
        var responseString = await response.Content.ReadAsStreamAsync();
        var instrumentResponse = await JsonSerializer.DeserializeAsync<InstrumentResponse>(responseString);
        if (instrumentResponse == null)
        {
            return [];
            // UpdateInstruments(instrumentResponse.Data);
        }
        return instrumentResponse.Data;
    }

    public async Task LoadProviders()
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

    public async Task LoadExchanges()
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
            var providerResponse = await JsonSerializer.DeserializeAsync<ProviderResponse>(responseString);
            if (providerResponse != null)
                UpdateProviders(providerResponse.Data);
        }
    }

    // public void UpdateInstruments(List<Instrument> data)
    // {
    //     lock (_lock)
    //     {
    //         foreach (var instrument in data)
    //         {
    //             instruments[instrument.Symbol] = instrument;
    //             if (!symbols.Contains(instrument.Symbol))
    //             {
    //                 symbols.Add(instrument.Symbol);
    //             }
    //         }
    //     }
    // }

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