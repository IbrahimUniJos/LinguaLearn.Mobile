using LinguaLearn.Mobile.ViewModels;
using LinguaLearn.Mobile.ViewModels.Auth;

namespace LinguaLearn.Mobile.Views.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(SignupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Set focus to display name field when page appears
        DisplayNameField.Focus();
    }
}