using Refit;
using LinguaLearn.Mobile.Models.Auth;

namespace LinguaLearn.Mobile.Services.Auth;

[Headers("Content-Type: application/json")]
public interface IFirebaseAuthApi
{
    [Post("/signUp")]
    Task<SignUpResponse> SignUpAsync([Body] SignUpRequest request, [Query] string key);
    
    [Post("/signInWithPassword")]
    Task<SignInResponse> SignInAsync([Body] SignInRequest request, [Query] string key);
    
    [Post("/token")]
    Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request, [Query] string key);
    
    [Post("/update")]
    Task<UpdateProfileResponse> UpdateProfileAsync([Body] UpdateProfileRequest request, [Query] string key);
    
    [Post("/delete")]
    Task<DeleteAccountResponse> DeleteAccountAsync([Body] DeleteAccountRequest request, [Query] string key);
    
    [Post("/sendOobCode")]
    Task<SendOobCodeResponse> SendPasswordResetAsync([Body] SendOobCodeRequest request, [Query] string key);
    
    [Post("/resetPassword")]
    Task<ResetPasswordResponse> ResetPasswordAsync([Body] ResetPasswordRequest request, [Query] string key);
}