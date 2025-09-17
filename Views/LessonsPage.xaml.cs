using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

public partial class LessonsPage : ContentPage
{
    private readonly LessonsViewModel _viewModel;

    public LessonsPage(LessonsViewModel viewModel)
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