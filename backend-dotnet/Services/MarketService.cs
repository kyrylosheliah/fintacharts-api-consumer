using BackendDotnet.Data;
using BackendDotnet.Models;

namespace BackendDotnet.Services;

public class MarketService(AssetCache _assetCache)
{
    public List<string>? GetProviderList() => _assetCache.providers;

    public List<string>? GetExchangeList(string provider) =>
        _assetCache.exchanges[provider];

    public async Task<List<Instrument>> GetAssetList(
        string? symbol,
        int? page,
        int? size,
        string? provider
    )
    {
        var instruments = await _assetCache.GetInstruments(symbol, page, size, provider);
        return instruments;
    }
}
