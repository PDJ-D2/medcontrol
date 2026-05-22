using System.Text.Encodings.Web;
using MedControl.Api.Domain;
using MedControl.Api.Infrastructure.Auth;
using MedControl.Api.Infrastructure.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace MedControl.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            UserManager<AppUser> userManager,
            JwtTokenService tokenService) =>
        {
            var validationError = ValidateRegister(request);
            if (validationError is not null)
            {
                return Results.BadRequest(new { error = validationError });
            }

            var user = new AppUser
            {
                Email = request.Email.Trim(),
                UserName = request.UserName.Trim(),
                DisplayName = request.UserName.Trim(),
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { error = ToMessage(result) });
            }

            return Results.Ok(ToAuthResponse(user, tokenService));
        });

        group.MapPost("/login", async (
            LoginRequest request,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtTokenService tokenService) =>
        {
            var user = request.EmailOrUserName.Contains('@')
                ? await userManager.FindByEmailAsync(request.EmailOrUserName.Trim())
                : await userManager.FindByNameAsync(request.EmailOrUserName.Trim());

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(ToAuthResponse(user, tokenService));
        });

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            return Results.Ok(new
            {
                id = user.GetUserId(),
                email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                userName = user.Identity?.Name
            });
        }).RequireAuthorization();

        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email.Trim());
            if (user is null)
            {
                return Results.Ok(new { message = "Se o email existir, enviaremos as instrucoes." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = configuration["ClientApp:BaseUrl"] ?? "http://localhost:5173";
            var resetUrl = $"{baseUrl}/reset-password?email={UrlEncoder.Default.Encode(user.Email!)}&token={encodedToken}";

            await emailSender.SendAsync(
                user.Email!,
                "Recuperacao de senha - MedControl",
                $"Use este link para redefinir sua senha: {resetUrl}",
                cancellationToken);

            return Results.Ok(new { message = "Se o email existir, enviaremos as instrucoes." });
        });

        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            UserManager<AppUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email.Trim());
            if (user is null)
            {
                return Results.BadRequest(new { error = "Token invalido ou expirado." });
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

            return result.Succeeded
                ? Results.Ok(new { message = "Senha redefinida com sucesso." })
                : Results.BadRequest(new { error = ToMessage(result) });
        });

        return app;
    }

    private static AuthResponse ToAuthResponse(AppUser user, JwtTokenService tokenService)
    {
        return new AuthResponse(
            tokenService.CreateToken(user),
            new UserResponse(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty));
    }

    private static string? ValidateRegister(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName))
        {
            return "Informe email e nome de usuario.";
        }

        return string.IsNullOrWhiteSpace(request.Password)
            ? "Informe uma senha."
            : null;
    }

    private static string ToMessage(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(e => e.Description));
    }
}
