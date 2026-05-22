namespace MedControl.Api.Features.Auth;

public sealed record RegisterRequest(string Email, string UserName, string Password);

public sealed record LoginRequest(string EmailOrUserName, string Password);

public sealed record AuthResponse(string Token, UserResponse User);

public sealed record UserResponse(string Id, string Email, string UserName);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
