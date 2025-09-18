using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

public partial class LessonPlayerPage : ContentPage
{
    private readonly LessonPlayerViewModel _viewModel;

    public LessonPlayerPage(LessonPlayerViewModel viewModel)
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

    protected override bool OnBackButtonPressed()
    {
        // Show confirmation dialog before leaving lesson
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var result = await DisplayAlert(
                "Leave Lesson", 
                "Are you sure you want to leave this lesson? Your progress will be saved.", 
                "Leave", 
                "Stay");
                
            if (result)
            {
                await Shell.Current.GoToAsync("..");
            }
        });
        
        return true; // Prevent default back behavior
    }
}