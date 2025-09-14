namespace LinguaLearn.Mobile.Models.Auth;

public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string IdToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken) && DateTime.UtcNow < ExpiresAt;
}