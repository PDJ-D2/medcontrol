using System.Security.Claims;

namespace MedControl.Api.Infrastructure.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Usuario autenticado sem identificador.");
    }
}
