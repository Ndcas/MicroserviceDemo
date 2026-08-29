using StackExchange.Redis;
using System.Text.Json;

namespace user.Services
{
    public interface ICacheService
    {
        Task Set<T>(string key, T value, TimeSpan? expiry = null);

        Task<T?> Get<T>(string key);

        Task<bool> Remove(string key);
    }

    public class CacheService : ICacheService
    {
        private IDatabase _redis;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            string json = JsonSerializer.Serialize(value);

            await _redis.StringSetAsync(key, json, expiry, When.Always);
        }

        public async Task<T?> Get<T>(string key)
        {
            RedisValue value = await _redis.StringGetAsync(key);

            if (value == RedisValue.Null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>((string)value!);
        }

        public async Task<bool> Remove(string key)
        {
            return await _redis.StringDeleteAsync(key, When.Always);
        }
    }
}
