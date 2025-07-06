using System.Text.Json;
using BackendDotnet.Models;

namespace BackendDotnet.Services;

public class MarketService(HttpClient _httpClient, IConfiguration _config, AuthTokenService tokenService)
{
    private readonly string _baseUri = _config["Base:Uri"] ?? "";
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Fintacharts:Token"]}");

    public async Task<List<MarketAssetDto>> GetAssetListAsync()
    {
        var providers = await _httpClient.GetFromJsonAsync<ProviderResponse>($"{_baseUri}/api/instruments/v1/providers");
        var exchanges = await _httpClient.GetFromJsonAsync<ExchangeResponse>($"{_baseUri}/api/instruments/v1/exchanges");

        var marketAssets = new List<MarketAssetDto>();

        foreach (var provider in providers.Data)
        {
            string url = $"{_baseUri}/api/instruments/v1/instruments?provider={provider}&kind=forex";
            var instrumentResponse = await _httpClient.GetFromJsonAsync<JsonElement>(url);
            var dataElement = instrumentResponse.GetProperty("data");

            foreach (var element in dataElement.EnumerateArray())
            {
                var symbol = element.GetProperty("symbol").GetString();
                var description = element.GetProperty("description").GetString();
                var kind = element.GetProperty("kind").GetString();
                var currency = element.GetProperty("currency").GetString();
                var baseCurrency = element.GetProperty("baseCurrency").GetString();

                if (element.TryGetProperty("mappings", out var mappings) &&
                    mappings.TryGetProperty(provider, out var mapping))
                {
                    string providerSymbol = mapping.GetProperty("symbol").GetString();
                    string exchange = mapping.TryGetProperty("exchange", out var ex) ? ex.GetString() : null;

                    marketAssets.Add(new MarketAssetDto
                    {
                        Symbol = providerSymbol,
                        Description = description,
                        Kind = kind,
                        Currency = currency,
                        BaseCurrency = baseCurrency,
                        Provider = provider,
                        Exchange = exchange
                    });
                }
            }
        }

        return marketAssets;
    }
}
