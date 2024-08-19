
using Microsoft.AspNetCore.Http.Extensions;

namespace tinyidp.WebAuthent.Modules;

public class BasicAuthHttpMiddleware
{
    private readonly RequestDelegate _next;

    public BasicAuthHttpMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.GetDisplayUrl().Contains("/authorize"))
        {
            if (!IsBasicAuthent(context) && !(context.User?.Identity?.IsAuthenticated??false))
            {
                if (request.Cookies["tinyidp"] == null)
                {

                    response.Cookies.Append("tinyidp", "first-redirect");
                    response.Headers.Append("WWW-Authenticate",
                        string.Format("Basic realm=\"{0}\"", "https://localhost:7034"));
                    response.StatusCode = 401;
                    return;
                }
            }
        }

        await _next.Invoke(context);

        // Clean up.
    }

    private bool IsBasicAuthent(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (String.IsNullOrEmpty(authHeader))
            return false;

        if (!authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}

public static class BasicAuthHttpMiddlewareExtension
{
    public static IApplicationBuilder UseBasicAuthHttpMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<BasicAuthHttpMiddleware>();
    }
}