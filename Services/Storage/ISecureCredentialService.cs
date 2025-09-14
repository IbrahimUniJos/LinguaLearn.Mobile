namespace LinguaLearn.Mobile.Services.Storage;

public interface ISecureCredentialService
{
    Task<string?> GetFirebaseApiKeyAsync();
    Task SetFirebaseApiKeyAsync(string apiKey);
    Task<string?> GetIdTokenAsync();
    Task SetIdTokenAsync(string idToken);
    Task<string?> GetRefreshTokenAsync();
    Task SetRefreshTokenAsync(string refreshToken);
    Task ClearAllTokensAsync();
    Task<bool> HasValidCredentialsAsync();
}