using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultLib
{
    public class CacheAsideHelper
    {
        public static string GetKey(params string[] keys)
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

        public static void RemoveKey(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public static void Add(object item, string key)
        {
            var wrapper = new CacheItemWrapper
            {
                InsertedAt = DateTime.Now,
                Item = item
            };
            MemoryCache.Default.Add(key, wrapper, ObjectCache.InfiniteAbsoluteExpiration);
        }

        public static T GetOrAdd<T>(Func<T> builder, TimeSpan expiresIn, params string[] keys)
        {
            var key = GetKey(keys);
            CacheItemWrapper itemWrapper = null;
            var cachedItem = MemoryCache.Default.Get(key);
            if (cachedItem != null)
            {
                itemWrapper = (CacheItemWrapper)cachedItem;
            }

            if (itemWrapper != null && !(DateTime.Now.Subtract(itemWrapper.InsertedAt).TotalSeconds >=
                                         expiresIn.TotalSeconds))
            {
                return (T)itemWrapper.Item;
            }

            ////expired or not in cache
            var item = builder();
            var wrapper = new CacheItemWrapper
            {
                InsertedAt = DateTime.Now,
                Item = item
            };
            MemoryCache.Default.Add(key, wrapper, DateTime.Now.AddSeconds(expiresIn.TotalSeconds));
            return item;
        }
        public static T GetOrAdd<T>(Func<Task<T>> builder, TimeSpan expiresIn, params string[] keys)
        {
            var key = GetKey(keys);
            CacheItemWrapper itemWrapper = null;
            var cahedItem = MemoryCache.Default.Get(key);
            if (cahedItem != null)
            {
                itemWrapper = (CacheItemWrapper)cahedItem;
            }

            if (itemWrapper == null || DateTime.Now.Subtract(itemWrapper.InsertedAt).TotalSeconds >= expiresIn.TotalSeconds)
            {
                ////expired or not in cache
                var item = builder().Result;
                var wrapper = new CacheItemWrapper
                {
                    InsertedAt = DateTime.Now,
                    Item = item
                };
                MemoryCache.Default.Add(key, wrapper, DateTime.Now.AddSeconds(expiresIn.TotalSeconds));
                return item;
            }

            return (T)itemWrapper.Item;
        }

        private class CacheItemWrapper
        {
            public object Item { get; set; }
            public DateTime InsertedAt { get; set; }
        }

    }
}
