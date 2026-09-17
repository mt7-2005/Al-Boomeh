using Al_Boomeh.Authorization;
using Al_Boomeh.Services;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class StoreOwnerOrAdminHandler
    : AuthorizationHandler<StoreOwnerOrAdminRequirement, int>
{
    private readonly IUsersService _usersService;
    public StoreOwnerOrAdminHandler(IUsersService usersService)
    {
        _usersService = usersService;
    }
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoreOwnerOrAdminRequirement requirement,
        int storeId)
    {

        // Admin override
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Ownership check
        var userClaimId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var user =await _usersService.GetUser(Guid.Parse(userClaimId));

        if (user!=null && user.StoreId == storeId)
        {
            context.Succeed(requirement);
        }

        return;
    }
}
