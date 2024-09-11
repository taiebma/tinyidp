using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using tinyidp.Business.BusinessEntities;
using tinyidp.Exceptions;

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

    public static ECDsaSecurityKey GetEccSecurityKey(this KidBusinessEntity kid) 
    {
        ECDsa ecc = ECDsa.Create();
        ecc.ImportFromPem(kid.PublicKey.ToCharArray());
        return new ECDsaSecurityKey(ecc);
    }

    public static ECDsaSecurityKey GetEccPrivateSecurityKey(this KidBusinessEntity kid) 
    {
        ECDsa ecc = ECDsa.Create();
        ecc.ImportFromPem(kid.PrivateKey.ToCharArray());
        return new ECDsaSecurityKey(ecc);
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

    public static BasicIdent GetBasicIdent(this HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (authHeader == null)
            throw new TinyidpTokenException("No Authorization header", "invalid_request");

        if (!authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            throw new TinyidpTokenException("For client_credential grant_type, Authorization must be Basic ", "invalid_request");

        var parameters = authHeader.Substring("Basic ".Length);
        var authorizationKeys = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));

        var authorizationResult = authorizationKeys.IndexOf(':');
        if (authorizationResult == -1)
            throw new TinyidpTokenException("Basic Authorization must be <client_id>:<client_secret> format", "invalid_request");

        BasicIdent ident = new BasicIdent();
        ident.ClientId = authorizationKeys.Substring(0, authorizationResult);
        ident.ClientSecret = authorizationKeys.Substring(authorizationResult + 1);

        return ident;
    }

}