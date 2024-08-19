using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using tinyidp.Business.BusinessEntities;

namespace tinyidp.Extensions;

public static class ToolsExtensions
{

    public static byte[] GetHash(this string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(this string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }    

    public static RsaSecurityKey GetRsaSecurityKey(this KidBusinessEntity kid) 
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(kid.PublicKey.ToCharArray());
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey GetRsaPrivateSecurityKey(this KidBusinessEntity kid) 
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(kid.PrivateKey.ToCharArray());
        return new RsaSecurityKey(rsa);
    }

    public static string GetLocalUrl(this IUrlHelper urlHelper, string? localUrl)
    {
        if (localUrl == null || !urlHelper.IsLocalUrl(localUrl))
        {
            return urlHelper.Page("/Index")??"/Index";
        }

        return localUrl;
    }

    public static RoleCredential Role(this IEnumerable<Claim> claims)
    {
        Claim claimRole = claims.First(p => p.Type == "Role");
        return (RoleCredential)Enum.Parse(typeof(RoleCredential), claimRole.Value);
    }
}