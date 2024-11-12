using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Pinewood.Models;

namespace Pinewood.Middleware
{
    public class VisitorMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey("VisitorId"))
            {
                string visitorId = Guid.NewGuid().ToString();

                context.Response.Cookies.Append("VisitorId", visitorId, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true
                });                               
            }

            await _next(context);
        }
    }
}
