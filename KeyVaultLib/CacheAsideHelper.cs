using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultLib;

public class CacheAsideHelper : ICacheAsideHelper
{
    private static readonly Lazy<CacheAsideHelper> _instance = new(() => new CacheAsideHelper());

    public static CacheAsideHelper Instance => _instance.Value;

    public CacheAsideHelper() { }

    public string GetKey(params string[] keys)
    {
        var sb = new StringBuilder();
        var index = 0;
        foreach (var t in keys)
        {
            sb.AppendFormat((index > 0 ? ":" : string.Empty) + t.ToLower());
            index++;
        }
        return sb.ToString();
    }

    public void RemoveKey(string key)
    {
        MemoryCache.Default.Remove(key);
    }

    public void Add(object item, string key)
    {
        var wrapper = new CacheItemWrapper
        {
            InsertedAt = DateTime.Now,
            Item = item
        };
        MemoryCache.Default.Add(key, wrapper, ObjectCache.InfiniteAbsoluteExpiration);
    }

    public T GetOrAdd<T>(Func<T> builder, TimeSpan expiresIn, params string[] keys)
    {
        var key = GetKey(keys);
        CacheItemWrapper itemWrapper = null;
        var cachedItem = MemoryCache.Default.Get(key);
        if (cachedItem != null)
        {
            itemWrapper = (CacheItemWrapper)cachedItem;
        }
        if (itemWrapper != null && !(DateTime.Now.Subtract(itemWrapper.InsertedAt).TotalSeconds >= expiresIn.TotalSeconds))
        {
            return (T)itemWrapper.Item;
        }
        var item = builder();
        var wrapper = new CacheItemWrapper
        {
            InsertedAt = DateTime.Now,
            Item = item
        };
        MemoryCache.Default.Add(key, wrapper, DateTime.Now.AddSeconds(expiresIn.TotalSeconds));
        return item;
    }

    public async Task<T> GetOrAddAsync<T>(Func<Task<T>> builder, TimeSpan expiresIn, params string[] keys)
    {
        var key = GetKey(keys);
        CacheItemWrapper itemWrapper = null;
        var cachedItem = MemoryCache.Default.Get(key);
        if (cachedItem != null)
        {
            itemWrapper = (CacheItemWrapper)cachedItem;
        }
        if (itemWrapper == null || DateTime.Now.Subtract(itemWrapper.InsertedAt).TotalSeconds >= expiresIn.TotalSeconds)
        {
            try
            {
                var item = await builder();
                var wrapper = new CacheItemWrapper
                {
                    InsertedAt = DateTime.Now,
                    Item = item
                };
                MemoryCache.Default.Add(key, wrapper, DateTime.Now.AddSeconds(expiresIn.TotalSeconds));
                return item;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }
        return (T)itemWrapper.Item;
    }

    private class CacheItemWrapper
    {
        public object Item { get; set; }
        public DateTime InsertedAt { get; set; }
    }
}
