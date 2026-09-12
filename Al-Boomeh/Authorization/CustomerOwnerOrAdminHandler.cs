using Al_Boomeh.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class CustomerOwnerOrAdminHandler
    : AuthorizationHandler<CustomerOwnerOrAdminRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomerOwnerOrAdminRequirement requirement,
        int customerId)
    {

        // Admin override
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Ownership check

        var customerIdClaim = context.User.FindFirstValue("customerId");

        if (int.TryParse(customerIdClaim, out var userCustomerId) && userCustomerId == customerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
