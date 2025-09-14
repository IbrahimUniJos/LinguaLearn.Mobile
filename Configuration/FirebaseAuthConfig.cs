namespace LinguaLearn.Mobile.Configuration;

public class FirebaseAuthConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://identitytoolkit.googleapis.com/v1/accounts";
    public int TokenRefreshThresholdMinutes { get; set; } = 5;
    public int HttpTimeoutSeconds { get; set; } = 30;
}