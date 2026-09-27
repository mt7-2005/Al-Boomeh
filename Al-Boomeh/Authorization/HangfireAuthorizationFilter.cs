using Al_BoomehDAL.Models;
using Hangfire.Dashboard;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        var roleClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return isAuthenticated && roleClaim == User.UserRole.Admin.ToString();
    }
}