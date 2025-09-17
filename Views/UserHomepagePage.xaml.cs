using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

public partial class UserHomepagePage : ContentPage
{
    private readonly UserHomepageViewModel _viewModel;

    public UserHomepagePage(UserHomepageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}