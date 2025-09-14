using LinguaLearn.Mobile.Models.Auth;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Auth;

public interface IFirebaseAuthService
{
    Task<ServiceResult<UserSession>> SignUpWithEmailAsync(string email, string password, string? displayName = null, CancellationToken ct = default);
    Task<ServiceResult<UserSession>> SignInWithEmailAsync(string email, string password, CancellationToken ct = default);
    Task<ServiceResult<UserSession>> RefreshTokenAsync(CancellationToken ct = default);
    Task<ServiceResult<UserSession>> UpdateProfileAsync(string? displayName = null, string? photoUrl = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> SendPasswordResetEmailAsync(string email, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteAccountAsync(CancellationToken ct = default);
    Task SignOutAsync();
    Task<UserSession?> GetCurrentSessionAsync();
    bool IsAuthenticated { get; }
    event EventHandler<UserSession?>? AuthStateChanged;
}