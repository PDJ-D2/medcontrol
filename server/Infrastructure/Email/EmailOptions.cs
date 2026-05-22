namespace MedControl.Api.Infrastructure.Email;

public sealed class EmailOptions
{
    public string From { get; set; } = "no-reply@medcontrol.local";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
