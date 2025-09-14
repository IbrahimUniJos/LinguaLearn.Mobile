using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Storage;

public class SecureCredentialService : ISecureCredentialService
{
    private readonly ISecureStorage _secureStorage;
    private readonly ILogger<SecureCredentialService> _logger;

    private const string FIREBASE_API_KEY = "firebase_api_key";
    private const string ID_TOKEN_KEY = "firebase_id_token";
    private const string REFRESH_TOKEN_KEY = "firebase_refresh_token";
    private const string TOKEN_EXPIRY_KEY = "firebase_token_expiry";

    public SecureCredentialService(
        ISecureStorage secureStorage,
        ILogger<SecureCredentialService> logger)
    {
        _secureStorage = secureStorage;
        _logger = logger;
    }

    public async Task<string?> GetFirebaseApiKeyAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(FIREBASE_API_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Firebase API key");
            return null;
        }
    }

    public async Task SetFirebaseApiKeyAsync(string apiKey)
    {
        try
        {
            await _secureStorage.SetAsync(FIREBASE_API_KEY, apiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store Firebase API key");
            throw;
        }
    }

    public async Task<string?> GetIdTokenAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(ID_TOKEN_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ID token");
            return null;
        }
    }

    public async Task SetIdTokenAsync(string idToken)
    {
        try
        {
            await _secureStorage.SetAsync(ID_TOKEN_KEY, idToken);
            await _secureStorage.SetAsync(TOKEN_EXPIRY_KEY, DateTime.UtcNow.AddHours(1).ToBinary().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store ID token");
            throw;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(REFRESH_TOKEN_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refresh token");
            return null;
        }
    }

    public async Task SetRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await _secureStorage.SetAsync(REFRESH_TOKEN_KEY, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store refresh token");
            throw;
        }
    }

    public Task ClearAllTokensAsync()
    {
        try
        {
            _secureStorage.Remove(ID_TOKEN_KEY);
            _secureStorage.Remove(REFRESH_TOKEN_KEY);
            _secureStorage.Remove(TOKEN_EXPIRY_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear tokens");
        }
        
        return Task.CompletedTask;
    }

    public async Task<bool> HasValidCredentialsAsync()
    {
        try
        {
            var idToken = await GetIdTokenAsync();
            var expiryString = await _secureStorage.GetAsync(TOKEN_EXPIRY_KEY);
            
            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(expiryString))
                return false;

            if (long.TryParse(expiryString, out var expiryBinary))
            {
                var expiry = DateTime.FromBinary(expiryBinary);
                return expiry > DateTime.UtcNow.AddMinutes(5); // 5 min buffer
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate credentials");
            return false;
        }
    }
}