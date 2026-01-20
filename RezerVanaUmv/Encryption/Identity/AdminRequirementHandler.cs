
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RezerVanaUmv.Encryption.Identity;
using Microsoft.AspNetCore.Authorization;
using RezerVanaUmv.Encryption.Identity;

public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
                if (context.User.IsInRole("ADMIN"))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
        }

        // If the user doesn't meet the requirement, fail the authorization
        context.Fail();
        return Task.CompletedTask;
    }
}
