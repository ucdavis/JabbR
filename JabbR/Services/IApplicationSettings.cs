
namespace JabbR.Services
{
    public interface IApplicationSettings
    {
        string AuthApiKey { get; }
        string RedisServer { get; }
        string RedisPassword { get; }
        int RedisPort { get; }
    }
}
