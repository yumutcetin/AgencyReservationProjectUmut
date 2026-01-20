using Microsoft.AspNetCore.Authorization;
using RezerVanaUmv.Identity;
using System.Security.Claims;

public class AgencyCrwAuthHandler : AuthorizationHandler<AgencyCrwAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgencyCrwAuthRequirement requirement)
    {

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            var claims = context.User.Claims
                .Where(c => c.Type ==  "AgencyId");

            foreach (var role in roles)
            {
                if (role.EndsWith("AGENCY") && claims.Any())
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
