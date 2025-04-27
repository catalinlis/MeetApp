using System.Security.Cryptography;
using System.Text;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Microsoft.AspNetCore.Diagnostics;

namespace API.Helpers;

public class CloudFrontSignedUrlGenerator{

    private readonly string CloudFrontPrivateKey;
    private string CloudFrontKeyPairId;

    public CloudFrontSignedUrlGenerator(string PrivateKey, string KeyPair){
        CloudFrontPrivateKey = PrivateKey;
        CloudFrontKeyPairId = KeyPair;
    }

    public string GenerateSignedUrl(string url, DateTime expirationTime){
        
        long expires = ((DateTimeOffset) expirationTime).ToUnixTimeSeconds();

        string policy = $"{{\"Statement\":[{{\"Resource\":\"{url}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expires}}}}}}}]}}";

        byte[] policyBytes = Encoding.UTF8.GetBytes(policy);

        using (RSA rsa = RSA.Create()){
            rsa.ImportFromPem(CloudFrontPrivateKey.ToCharArray());
            byte[] signedPolicy = rsa.SignData(policyBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        
            string base64SignedPolicy = Convert.ToBase64String(signedPolicy);

            string signedUrl = $"{url}?Expires={expires}&Signature={Uri.EscapeDataString(base64SignedPolicy)}&Key-Pair-Id={CloudFrontKeyPairId}";

            return signedUrl;
        }
        



    }
}