using BackendDotnet.Models;

namespace BackendDotnet.Data;

public class MarketQuoteCache
{
    private readonly Dictionary<string, MarketQuote> _cache = [];
    private readonly Lock _lock = new();

    public void Update(string symbol, decimal bid, decimal ask, decimal last)
    {
        lock (_lock)
        {
            _cache[symbol] = new MarketQuote
            {
                Symbol = symbol,
                Bid = bid,
                Ask = ask,
                Last = last,
                Timestamp = DateTime.UtcNow,
            };
        }
    }

    public bool TryGet(string symbol, out MarketQuote quote)
    {
        lock (_lock)
        {
            return _cache.TryGetValue(symbol, out quote);
        }
    }
}