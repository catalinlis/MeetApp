using API.Helpers;

namespace API.Services;

public class CloudFrontService{

    private string CloudFrontDomain;
    private string ResourcePath;
    private string KeyPairId;
    private string PrivateKey;

    public CloudFrontService(EnvironmentVariables variables){
        var privateKeyBase64 = variables["CLOUDFRONT_PRIVATE_KEY"];
        var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
        PrivateKey = System.Text.Encoding.UTF8.GetString(privateKeyBytes);
        KeyPairId = variables["CLOUDFRONT_KEY_PAIR_ID"];
        CloudFrontDomain = variables["CLOUDFRONT_BASE_URL"];
        ResourcePath = variables["CLOUDFRONT_RESOURCE_PATH"];
    }

    public string SignUrl(string resourceId){
        var url = $"{CloudFrontDomain}/{ResourcePath}/{resourceId}";
        var generator = new CloudFrontSignedUrlGenerator(PrivateKey, KeyPairId);
        DateTime expirationTime = DateTime.UtcNow.AddHours(1);
        string signedUrl = generator.GenerateSignedUrl(url, expirationTime);

        return signedUrl;
    }
}