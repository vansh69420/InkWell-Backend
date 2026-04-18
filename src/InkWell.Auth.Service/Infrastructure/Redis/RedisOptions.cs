namespace InkWell.Auth.Service.Infrastructure.Redis;

public class RedisOptions
{
    public string Configuration { get; set; } = string.Empty;
    public string BufferKey { get; set; } = "inkwell:auth:buffer";
    public string CachePrefix { get; set; } = "inkwell:auth:cache:";
}