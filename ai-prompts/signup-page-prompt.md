# AI Prompt for Signup Page Generation

## App Context
Create a signup/registration page for a language learning app called LinguaLearn using .NET MAUI with MVVM pattern. The app uses Firebase Authentication for user authentication.

## Technology Requirements
- .NET MAUI with C# 12
- MVVM pattern using CommunityToolkit.Mvvm
- Use Horus MAUI Material Design Controls (MaterialTextField, MaterialButton, etc.)
- Firebase Authentication service integration

## User Model
The user has the following properties:
- UserId (string)
- Email (string)
- DisplayName (string?)
- IdToken (string)
- RefreshToken (string)
- ExpiresAt (DateTime)

## Authentication Service Interface
The app uses IFirebaseAuthService with these methods:
- Task<ServiceResult<UserSession>> SignUpWithEmailAsync(string email, string password, string? displayName = null, CancellationToken ct = default)
- Task<ServiceResult<UserSession>> SignInWithEmailAsync(string email, string password, CancellationToken ct = default)
- Task SignOutAsync()
- Task<UserSession?> GetCurrentSessionAsync()
- bool IsAuthenticated { get; }

## UI Requirements

### Controls to Use
1. MaterialTextField for display name input
2. MaterialTextField for email input
3. MaterialTextField for password input (with IsPassword=true)
4. MaterialTextField for confirm password input (with IsPassword=true)
5. MaterialButton for signup button
6. MaterialButton for navigation to login page
7. MaterialActivityIndicator for loading state during registration
8. MaterialSnackbar for error messages
9. MaterialCheckbox for terms and conditions agreement (if needed)

### Layout Requirements
- Clean, modern design following Material Design 3 guidelines
- App logo at the top
- "Create Account" heading
- Display Name, Email, Password, and Confirm Password fields with proper validation
- "Sign Up" primary button
- "Already have an account? Login" link button
- Loading indicator when registration is in progress
- Proper error handling and user feedback
- Terms and conditions checkbox (optional but recommended)

### Workflow Requirements
1. User opens the signup page
2. User enters display name, email, password, and confirm password
3. User agrees to terms and conditions (if implemented)
4. User clicks "Sign Up" button
5. Form validates inputs locally:
   - Display name: Required
   - Email format validation
   - Password length validation (minimum 6 characters)
   - Password and confirm password match
6. If validation passes:
   - Show loading indicator
   - Call AuthService.SignUpWithEmailAsync()
7. If registration succeeds:
   - Automatically sign in the user
   - Navigate to onboarding or main app
8. If registration fails:
   - Hide loading indicator
   - Show error message in snackbar
9. "Login" button navigates to login page

### Validation Rules
- Display Name: Required
- Email: Required, valid email format
- Password: Required, minimum 6 characters
- Confirm Password: Required, must match Password

### Error Handling
- Network errors
- Email already in use
- Weak password
- Invalid email format
- Account disabled
- Too many requests

## Files to Generate
1. Views/Auth/RegisterPage.xaml (UI layout)
2. Views/Auth/RegisterPage.xaml.cs (Code-behind)
3. ViewModels/AuthViewModel.cs (Shared view model for auth operations - extend existing)
4. Update AppShell.xaml to register the route

## Specific Implementation Details
- Use MaterialTextField for input fields with proper styling
- Implement INotifyDataErrorInfo for validation in ViewModel
- Use RelayCommand from CommunityToolkit.Mvvm for commands
- Handle loading states with IsBusy property
- Use WeakReferenceMessenger for navigation events if needed
- Follow MVVM best practices (no business logic in code-behind)
- Use proper binding for all UI elements
- Implement proper accessibility attributes
- Support dark/light theme switching

## Example Structure
```
<ContentPage>
    <ScrollView>
        <VerticalStackLayout>
            <Image Source="app_logo.png" />
            <Label Text="Create Account" />
            <MaterialTextField x:Name="DisplayNameEntry" Text="{Binding DisplayName}" Placeholder="Display Name" />
            <MaterialTextField x:Name="EmailEntry" Text="{Binding Email}" Placeholder="Email" />
            <MaterialTextField x:Name="PasswordEntry" Text="{Binding Password}" Placeholder="Password" IsPassword="True" />
            <MaterialTextField x:Name="ConfirmPasswordEntry" Text="{Binding ConfirmPassword}" Placeholder="Confirm Password" IsPassword="True" />
            <MaterialCheckbox IsChecked="{Binding AgreeToTerms}" Text="I agree to the Terms and Conditions" IsVisible="{Binding ShowTermsCheckbox}" />
            <MaterialButton Text="Sign Up" Command="{Binding SignUpCommand}" IsEnabled="{Binding IsBusy, Converter={InvertedBoolConverter}}" />
            <MaterialActivityIndicator IsRunning="{Binding IsBusy}" IsVisible="{Binding IsBusy}" />
            <MaterialButton Text="Already have an account? Login" Command="{Binding NavigateToLoginCommand}" Style="{StaticResource TextButtonStyle}" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

## ViewModel Requirements
- Observable properties for DisplayName, Email, Password, ConfirmPassword, AgreeToTerms, IsBusy
- RelayCommand for SignUpCommand
- RelayCommand for NavigateToLoginCommand
- Validation logic for all fields
- Password matching validation
- Error handling and messaging
- Navigation logic after successful registration
- Terms and conditions validation (if implemented)