using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RezerVanaUmv.Encryption.Identity;

public class OtelCrwAuthRequirementHandler : AuthorizationHandler<OtelCrwAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OtelCrwAuthRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role) && role.Trim().EndsWith("/HTL", 
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }
}
