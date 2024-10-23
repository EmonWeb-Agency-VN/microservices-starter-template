namespace Common.Domain.Interfaces
{
    public interface ICustomMemoryCacheService
    {
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan cacheDuration);
        void Remove(string key);
    }
}
