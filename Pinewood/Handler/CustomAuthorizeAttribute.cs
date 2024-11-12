using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pinewood.Services; // Adjust namespace as needed
using System.Linq;
using System.Threading.Tasks;

public class CustomAuthorizeAttribute : TypeFilterAttribute
{
    public CustomAuthorizeAttribute(string role) : base(typeof(CustomAuthorizeFilter))
    {
        Arguments = new object[] { role };
    }
}

public class CustomAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly string _role;
    private readonly AuthApiClient _authApiClient;

    public CustomAuthorizeFilter(string role, AuthApiClient authApiClient)
    {
        _role = role;
        _authApiClient = authApiClient;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var (isAuthenticated, roles) = await _authApiClient.IsLoggedInAsync();

        if (!isAuthenticated || roles == null || !roles.Contains(_role))
        {
            context.Result = new RedirectToActionResult("Unauthorized", "Home", null);
        }
    }
}