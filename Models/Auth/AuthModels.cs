using System.Text.Json.Serialization;

namespace LinguaLearn.Mobile.Models.Auth;

public class SignUpRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

public class SignInRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

public class RefreshTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "refresh_token";
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

public class DeleteAccountRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
}

public class SendOobCodeRequest
{
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "PASSWORD_RESET";
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [JsonPropertyName("oobCode")]
    public string OobCode { get; set; } = string.Empty;
    
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = string.Empty;
}

public class SignUpResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

public class SignInResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("registered")]
    public bool Registered { get; set; }
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
}

public class UpdateProfileResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
}

public class DeleteAccountResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
}

public class SendOobCodeResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = string.Empty;
}