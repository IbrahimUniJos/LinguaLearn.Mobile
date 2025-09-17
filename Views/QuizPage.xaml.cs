using LinguaLearn.Mobile.ViewModels;

namespace LinguaLearn.Mobile.Views;

/// <summary>
/// Quiz page for interactive quizzes
/// </summary>
public partial class QuizPage : ContentPage
{
    public QuizPage(QuizViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Get quiz ID from query parameters
        if (BindingContext is QuizViewModel viewModel)
        {
            var quizId = GetQueryParameter("quizId");
            if (!string.IsNullOrEmpty(quizId))
            {
                await viewModel.InitializeAsync(quizId);
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Clean up timer when leaving page
        if (BindingContext is QuizViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }

    private string? GetQueryParameter(string key)
    {
        try
        {
            var uri = new Uri(Shell.Current.CurrentState.Location.ToString());
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query[key];
        }
        catch
        {
            return null;
        }
    }
}