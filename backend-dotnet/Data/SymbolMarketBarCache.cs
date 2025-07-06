using System.Diagnostics.CodeAnalysis;
using BackendDotnet.Models;

namespace BackendDotnet.Data;

public class SymbolMarketBarCache
{
    private readonly Dictionary<string, MarketBar> _cache = [];
    private readonly Lock _lock = new();

    public void Update(string symbol, MarketBar subject)
    {
        lock (_lock)
        {
            _cache[symbol] = subject;
        }
    }

    public bool TryGet(string symbol, [NotNullWhen(true)] out MarketBar? bar)
    {
        lock (_lock)
        {
            return _cache.TryGetValue(symbol, out bar);
        }
    }
}