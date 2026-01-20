using Microsoft.AspNetCore.Authorization;

namespace RezerVanaUmv.Encryption.Identity
{
    public class AdminRequirement: IAuthorizationRequirement
    {
        public AdminRequirement() { }
    }
}
