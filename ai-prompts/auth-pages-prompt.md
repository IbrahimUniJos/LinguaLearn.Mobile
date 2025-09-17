# AI Prompts for Login and Signup Pages with Workflows

## App Context
Create login and signup pages for a language learning app called LinguaLearn using .NET MAUI with MVVM pattern. The app uses Firebase Authentication for user authentication and Horus MAUI Material Design Controls for the UI.

## Technology Requirements
- .NET MAUI with C# 12+
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
1. MaterialTextField for text inputs
2. MaterialButton for action buttons
3. MaterialActivityIndicator for loading states
4. MaterialSnackbar for error messages
5. MaterialCheckbox for terms agreement (signup only)

### Common Layout Requirements
- Clean, modern design following Material Design 3 guidelines
- App logo at the top
- Loading indicator when authentication is in progress
- Proper error handling and user feedback

## Login Page Requirements

### Specific Controls
1. MaterialTextField for email input
2. MaterialTextField for password input (with IsPassword=true)
3. MaterialButton for login button
4. MaterialButton for navigation to signup page
5. MaterialActivityIndicator for loading state during authentication
6. MaterialSnackbar for error messages

### Layout
- "Welcome Back" heading
- Email and Password fields with proper validation
- "Login" primary button
- "Don't have an account? Sign Up" link button
- "Forgot Password?" link (optional)

### Workflow
1. User opens the login page
2. User enters email and password
3. User clicks "Login" button
4. Form validates inputs locally:
   - Email format validation
   - Password length validation (minimum 6 characters)
5. If validation passes:
   - Show loading indicator
   - Call AuthService.SignInWithEmailAsync()
6. If authentication succeeds:
   - Navigate to main app (lessons page)
7. If authentication fails:
   - Hide loading indicator
   - Show error message in snackbar
8. "Sign Up" button navigates to registration page

### Validation Rules
- Email: Required, valid email format
- Password: Required, minimum 6 characters

## Signup Page Requirements

### Specific Controls
1. MaterialTextField for display name input
2. MaterialTextField for email input
3. MaterialTextField for password input (with IsPassword=true)
4. MaterialTextField for confirm password input (with IsPassword=true)
5. MaterialButton for signup button
6. MaterialButton for navigation to login page
7. MaterialActivityIndicator for loading state during registration
8. MaterialSnackbar for error messages
9. MaterialCheckbox for terms and conditions agreement (optional)

### Layout
- "Create Account" heading
- Display Name, Email, Password, and Confirm Password fields with proper validation
- "Sign Up" primary button
- "Already have an account? Login" link button

### Workflow
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

## Error Handling
- Network errors
- Invalid credentials
- Account not found
- Account disabled
- Email already in use
- Weak password
- Too many requests

## Files to Generate
1. Views/Auth/LoginPage.xaml (UI layout)
2. Views/Auth/LoginPage.xaml.cs (Code-behind)
3. Views/Auth/RegisterPage.xaml (UI layout)
4. Views/Auth/RegisterPage.xaml.cs (Code-behind)
5. ViewModels/AuthViewModel.cs (Shared view model for auth operations)
6. Update AppShell.xaml to register the routes

## ViewModel Requirements
- Observable properties for Email, Password, DisplayName, ConfirmPassword, AgreeToTerms, IsBusy
- RelayCommand for LoginCommand, SignUpCommand, NavigateToLoginCommand, NavigateToSignUpCommand
- Validation logic for all fields
- Password matching validation
- Error handling and messaging
- Navigation logic after successful authentication/registration

## Implementation Details
- Use MaterialTextField for input fields with proper styling
- Implement INotifyDataErrorInfo for validation in ViewModel
- Use RelayCommand from CommunityToolkit.Mvvm for commands
- Handle loading states with IsBusy property
- Use WeakReferenceMessenger for navigation events if needed
- Follow MVVM best practices (no business logic in code-behind)
- Use proper binding for all UI elements
- Implement proper accessibility attributes
- Support dark/light theme switching