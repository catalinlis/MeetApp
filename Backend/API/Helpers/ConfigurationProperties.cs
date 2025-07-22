namespace API.Helpers;

public static class ConfigurationProperties
{
    public static string RedisConnection = "REDIS_CONNECTION";
    public static string DBConnection = "DEFAULT_DATABASE_CONNECTION";
    public static string CorsPolicy = "CORS_POLICY";
    public static string JwtSecret = "JWT_SECRET";
    public static string ValidIssuer = "VALID_ISSUERS";
    public static string ValidAudiences = "VALID_AUDIENCES";
    public static string AwsAccessKeyId = "AWS_ACCESS_KEY_ID";
    public static string AwsSecretKeyAccessId = "AWS_SECRET_KEY_ACCESS_ID";
    public static string AwsRegion = "AWS_REGION";
    public static string CloudfrontBaseUrl = "CLOUDFRONT_BASE_URL";
    public static string CloudfrontKeyPairId = "CLOUDFRONT_KEY_PAIR_ID";
    public static string CloudfrontPrivateKey = "CLOUDFRONT_PRIVATE_KEY";
    public static string DynamoDBTable = "DYNAMO_DATABASE_TABLE";
    public static string RabbitMqHostname = "RABBITMQ_HOSTNAME";
    public static string RabbitMqUsername = "RABBITMQ_USERNAME";
    public static string RabbitMqPassword = "RABBITMQ_PASSWORD";
    public static string SqsUrl = "SQS_URL";
}