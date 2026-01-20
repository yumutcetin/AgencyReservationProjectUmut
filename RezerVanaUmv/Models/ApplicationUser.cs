using Microsoft.AspNetCore.Identity;
using System;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChangeDate { get; set; }
}

