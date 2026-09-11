using System.Text.Json;
using StackExchange.Redis;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Cache;

public class CacheService : ICacheService
{
    private IDatabase _database;

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value == RedisValue.Null)
        {
            return default(T);
        }

        return JsonSerializer.Deserialize<T>((string)value);
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.StringDeleteAsync(key, When.Always);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var jsonValue = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(key, jsonValue, expiry, When.Always);
    }
}
