using System.Runtime.Caching;
using Pinewood.Models;

namespace Pinewood.Services
{
    public class CacheService : ICacheService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private static readonly CacheItemPolicy _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60) };

        public void Update<T>(string key, List<T> Data)
        {
            _cache.Set(key, Data, _policy);
        }

        public List<T> Get<T>(string key)
        {
            if (_cache.Contains(key))
            {
                return _cache.Get(key) as List<T>;
            }

            return new List<T>();
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }

}
