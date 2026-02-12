
using Microsoft.Extensions.Caching.Memory;
using tinyidp.infrastructure.bdd;

public class MemoryCacheTinyidp<T> : ICacheTinyidp<T> 
where T : ICachable
{
    private IMemoryCache _cache;
    
    public MemoryCacheTinyidp(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set(string key, T value)
    {
        var expirationTime = value.Expiration;
        if (expirationTime.HasValue)
        {
            DateTime curDate = DateTime.Now;
            _cache.Set(key, value, expirationTime.Value - curDate);
        }
        else
        {
            _cache.Set(key, value);
        }
    }

    public T? Get(string key)
    {
        T? entry;
        if (!_cache.TryGetValue<T>(key, out entry))
        {
            return default(T?);
        }
        return entry;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}