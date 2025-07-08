using System.Net.Http.Headers;
using System.Text.Json;
using BackendDotnet.Data;
using BackendDotnet.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace BackendDotnet.Services;

public class AssetService(
    AssetCache _assetCache,
    AuthTokenService _tokenService,
    IConfiguration _config,
    HttpClient _httpClient
)
{
    private readonly string _baseUri = _config["Base:Url"] ?? "";

    public List<string>? GetProviderList() => _assetCache.providers;

    public Dictionary<string, List<string>> GetExchanges() =>
        _assetCache.exchanges;

    public List<string>? GetExchangeList(string provider) =>
        _assetCache.exchanges[provider];

    public Instrument? GetInstrument(string symbol)
    {
        var cacheValue = _assetCache.instruments[symbol];
        if (cacheValue != null)
            return cacheValue;
        var response = GetInstruments(symbol, null, null, null);
        if (response == null)
            return null;
        cacheValue = _assetCache.instruments[symbol];
        return cacheValue;
    }

    public async Task<InstrumentResponse?> GetInstruments(
        string? symbol,
        int? page,
        int? size,
        string? provider
    )
    {
        var bearerToken = await _tokenService.GetAccessTokenAsync();
        if (bearerToken == null)
            return null;
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
            return null;
        var responseStream = await response.Content.ReadAsStreamAsync();
        var instrumentResponse = await JsonSerializer.DeserializeAsync<InstrumentResponse>(responseStream);
        if (instrumentResponse == null)
            return null;
        _assetCache.UpdateInstruments(instrumentResponse.Data);
        return instrumentResponse;
    }

    public async Task<MarketBarResponse?> GetHistory(
        string symbol,
        string? provider,
        int? interval,
        string? periodicity,
        DateOnly? startDate,
        DateOnly? endDate
    )
    {
        var bearerToken = await _tokenService.GetAccessTokenAsync();
        if (bearerToken == null)
            return null;
        var instrument = GetInstrument(symbol);
        if (instrument == null)
            return null;
        string url = $"{_baseUri}/api/bars/v1/bars/date-range";
        var query = new Dictionary<string, string?>()
        {
            ["instrumentId"] = instrument.Id,
            ["provider"] = provider,
            ["interval"] = interval?.ToString(),
            ["periodicity"] = periodicity,
            ["startDate"] = startDate?.ToString(),
            ["endDate"] = endDate?.ToString(),
        };
        var uri = QueryHelpers.AddQueryString(url, query);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;
        var responseStream = await response.Content.ReadAsStreamAsync();
        var historyResponse = await JsonSerializer.DeserializeAsync<MarketBarResponse>(responseStream);
        if (historyResponse == null)
            return null;
        return historyResponse;
    }
}
