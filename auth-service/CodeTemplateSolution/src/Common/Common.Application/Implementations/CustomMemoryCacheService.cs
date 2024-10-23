using Common.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Common.Application.Implementations
{
    public class CustomMemoryCacheService(IMemoryCache memoryCache) : ICustomMemoryCacheService
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan cacheDuration)
        {
            if (memoryCache.TryGetValue(key, out T cachedValue))
            {
                return cachedValue;
            }

            // Create a SemaphoreSlim if it doesn't already exist
            var myLock = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

            // Acquire the lock
            await myLock.WaitAsync();
            try
            {
                // Double-check if the item was added to the cache by another thread
                if (memoryCache.TryGetValue(key, out cachedValue))
                {
                    return cachedValue;
                }

                // Fetch the data from the source
                var result = await factory();

                // Store the data in the cache with the specified duration
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDuration
                };

                memoryCache.Set(key, result, cacheEntryOptions);

                return result;
            }
            finally
            {
                // Release the lock
                myLock.Release();
                // Clean up old locks
                _locks.TryRemove(key, out _);
            }
        }

        public void Remove(string key)
        {
            memoryCache.Remove(key);
        }
    }
}
