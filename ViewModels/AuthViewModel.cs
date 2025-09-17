// This file has been superseded by separate view models:
// - ViewModels/Auth/LoginViewModel.cs
// - ViewModels/Auth/SignupViewModel.cs
// Kept temporarily to avoid breaking references; consider removing after updating views.

using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinguaLearn.Mobile.ViewModels;

public partial class AuthViewModel : ObservableValidator
{
}