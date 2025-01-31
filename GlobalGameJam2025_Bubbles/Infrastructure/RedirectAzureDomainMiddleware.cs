using Microsoft.AspNetCore.Http.Extensions;

namespace GlobalGameJam2025_Bubbles.Infrastructure;

public class RedirectAzureDomainMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _customDomain;

    public RedirectAzureDomainMiddleware(RequestDelegate next, string? customDomain)
    {
        _next = next;
        _customDomain = customDomain;
    }

    public async Task Invoke(HttpContext context)
    {
        // If no custom domain is set, just continue down the pipeline
        if (string.IsNullOrWhiteSpace(_customDomain))
        {
            await _next(context);
            return;
        }

        // Check if the request host ends with "azurewebsites.net"
        if (context.Request.Host.Host.EndsWith("azurewebsites.net", StringComparison.OrdinalIgnoreCase))
        {
            // Preserve the path and query by re-using the original URL
            var uriBuilder = new UriBuilder(context.Request.GetEncodedUrl())
            {
                Host = _customDomain,
                Scheme = Uri.UriSchemeHttps,
                Port = -1 // Clear the port so the default (443) is used
            };

            // Perform a permanent redirect (301)
            context.Response.Redirect(uriBuilder.Uri.ToString(), true);
            Console.WriteLine($"Redirecting to custom domain: {_customDomain}");
            return;
        }

        // Otherwise, carry on
        await _next(context);
    }
}

public static class RedirectAzureDomainMiddlewareExtensions
{
    public static IApplicationBuilder UseRedirectAzureDomain(
        this IApplicationBuilder builder,
        string? customDomain)
    {
        return builder.UseMiddleware<RedirectAzureDomainMiddleware>(customDomain);
    }
}