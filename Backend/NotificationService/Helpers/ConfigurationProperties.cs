namespace Notification.Helpers;

public static class ConfigurationProperties{
    public static string RedisConnection = "REDIS_CONNECTION";
    public static string DBConnection = "DEFAULT_DATABASE_CONNECTION";
    public static string CorsPolicy = "CORS_POLICY";
    public static string JwtSecret = "JWT_SECRET";
    public static string ValidIssuer = "VALID_ISSUERS";
    public static string ValidAudiences = "VALID_AUDIENCES";
}