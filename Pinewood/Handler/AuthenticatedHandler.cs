using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class AuthenticatedHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _referrer;

    public AuthenticatedHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _referrer = "https://web.pinewood.co.uk";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Referrer = new Uri(_referrer);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("Unable to access HttpContext.");
        }

        var token = httpContext.Request.Cookies["PinewoodAuthToken"];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            throw new UnauthorizedAccessException("Authentication token not found in cookies.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

