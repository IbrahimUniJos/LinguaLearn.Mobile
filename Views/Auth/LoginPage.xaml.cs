using LinguaLearn.Mobile.ViewModels;
using LinguaLearn.Mobile.ViewModels.Auth;

namespace LinguaLearn.Mobile.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Set focus to email field when page appears
        EmailField.Focus();
    }
}