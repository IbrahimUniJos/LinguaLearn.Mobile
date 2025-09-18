using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

public partial class LessonDetailsPage : ContentPage
{
    private readonly LessonDetailsViewModel _viewModel;

    public LessonDetailsPage(LessonDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // ViewModel handles initialization through IQueryAttributable
    }
}