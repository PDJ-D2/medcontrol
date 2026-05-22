using Microsoft.AspNetCore.Identity;

namespace MedControl.Api.Domain;

public sealed class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
