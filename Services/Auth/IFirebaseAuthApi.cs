using Refit;
using LinguaLearn.Mobile.Models.Auth;

namespace LinguaLearn.Mobile.Services.Auth;

[Headers("Content-Type: application/json")]
public interface IFirebaseAuthApi
{
    /// <summary>
    /// Sign up a new user with email and password
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}
    /// </summary>
    [Post("/accounts:signUp?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<SignUpResponse> SignUpAsync(
        [Body] SignUpRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sign in an existing user with email and password
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}
    /// </summary>
    [Post("/accounts:signInWithPassword?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<SignInResponse> SignInAsync(
        [Body] SignInRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update user profile information
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:update?key={apiKey}
    /// </summary>
    [Post("/accounts:update?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<UpdateProfileResponse> UpdateProfileAsync(
        [Body] UpdateProfileRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete user account
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:delete?key={apiKey}
    /// </summary>
    [Post("/accounts:delete?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<DeleteAccountResponse> DeleteAccountAsync(
        [Body] DeleteAccountRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send password reset email
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}
    /// </summary>
    [Post("/accounts:sendOobCode?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<SendOobCodeResponse> SendPasswordResetAsync(
        [Body] SendOobCodeRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset password using OOB code
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:resetPassword?key={apiKey}
    /// </summary>
    [Post("/accounts:resetPassword?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<ResetPasswordResponse> ResetPasswordAsync(
        [Body] ResetPasswordRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user account information
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={apiKey}
    /// </summary>
    [Post("/accounts:lookup?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<GetAccountInfoResponse> GetAccountInfoAsync(
        [Body] GetAccountInfoRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send email verification
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}
    /// </summary>
    [Post("/accounts:sendOobCode?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<SendOobCodeResponse> SendEmailVerificationAsync(
        [Body] SendEmailVerificationRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirm email verification
    /// Firebase Auth REST API: https://identitytoolkit.googleapis.com/v1/accounts:update?key={apiKey}
    /// </summary>
    [Post("/accounts:update?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<UpdateProfileResponse> ConfirmEmailVerificationAsync(
        [Body] ConfirmEmailVerificationRequest request, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Separate interface for token refresh operations that use a different endpoint
/// Token refresh uses: https://securetoken.googleapis.com/v1/token?key={apiKey}
/// </summary>
[Headers("Content-Type: application/x-www-form-urlencoded")]
public interface IFirebaseTokenApi
{
    /// <summary>
    /// Refresh an ID token using a refresh token
    /// Firebase Token API: https://securetoken.googleapis.com/v1/token?key={apiKey}
    /// </summary>
    [Post("/token?key=AIzaSyD-HJhUBD8ZUPV3jORv4hiWqhmxgDUVF0U")]
    Task<RefreshTokenResponse> RefreshTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)] RefreshTokenRequest request, 
        CancellationToken cancellationToken = default);
}