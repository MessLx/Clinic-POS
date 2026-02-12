using System.Text.Json;
using StackExchange.Redis;

namespace PClinicPOS.Api.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().FirstOrDefault()!);
        if (server == null) return;
        await foreach (var key in server.KeysAsync(pattern: prefix + "*"))
        {
            await _redis.GetDatabase().KeyDeleteAsync(key);
        }
    }
}
