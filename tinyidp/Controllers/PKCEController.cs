using Microsoft.AspNetCore.Mvc;
using tinyidp.Encryption;

namespace tinyidp.Controllers;

public class PkceController
{

    public static async Task<IResult> GenerateVerifierCode(IPKCEService pkceService,int? size)
    {
        return Results.Ok(pkceService.GenerateVerifierCode(size??32));
    }

    public static async Task<IResult> GenerateChallenge(IPKCEService pkceService, [FromQuery] string code)
    {
        return Results.Ok(pkceService.GenerateCodeChallenge(code));
    }
}