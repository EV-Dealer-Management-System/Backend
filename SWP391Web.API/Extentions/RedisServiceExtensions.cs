using StackExchange.Redis;

namespace SWP391Web.API.Extentions
{
    public static class RedisServiceExtensions
    {
        public static WebApplicationBuilder AddRedisCacheService(this WebApplicationBuilder builder)
        {
            string connectionString = builder.Configuration.GetSection("Redis__ConnectionString").Value
                ?? throw new InvalidOperationException("Redis connection string is not configured.");

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            return builder;
        }
    }
}
