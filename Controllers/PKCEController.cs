using Microsoft.AspNetCore.Mvc;
using tinyidp.Encryption;

namespace tinyidp.Controllers;

[ApiController]
[Route("[controller]")]
public class PkceController : Controller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IPKCEService _pkceService;

    public PkceController(ILogger<PkceController> logger, IConfiguration configuration, IPKCEService pkceService)
    {
        _logger = logger;
        _configuration = configuration;
        _pkceService = pkceService;

    }

    [HttpGet]
    [Route("GenerateVerifierCode")]
    public IActionResult GenerateVerifierCode(int? size)
    {
        return Ok(_pkceService.GenerateVerifierCode(size??32));
    }

    [HttpGet]
    [Route("GenerateChallenge")]
    public IActionResult GenerateChallenge([FromQuery] string code)
    {
        return Ok(_pkceService.GenerateCodeChallenge(code));
    }
}