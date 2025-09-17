namespace LinguaLearn.Mobile.Views;

[QueryProperty(nameof(XPEarned), "xp")]
[QueryProperty(nameof(LessonId), "lessonId")]
[QueryProperty(nameof(LessonTitle), "title")]
public partial class LessonCompletePage : ContentPage
{
    public string XPEarned { get; set; } = "0";
    public string LessonId { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;

    public LessonCompletePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update lesson title
        if (!string.IsNullOrEmpty(LessonTitle))
        {
            LessonTitleLabel.Text = Uri.UnescapeDataString(LessonTitle);
        }

        // Update XP earned
        if (int.TryParse(XPEarned, out var xp))
        {
            XPEarnedLabel.Text = $"+{xp} XP Earned";
        }

        // Set placeholder values for other stats
        // In a real implementation, these would be passed as parameters or loaded from the service
        SectionsCompletedLabel.Text = "5";
        TimeSpentLabel.Text = "8 min";
        AccuracyLabel.Text = "95%";

        // TODO: Load and display any new badges earned
        // For now, hide the achievements section
        AchievementsSection.IsVisible = false;
    }

    private async void OnContinueLearningClicked(object sender, EventArgs e)
    {
        // Navigate to the next recommended lesson or back to lessons list
        await Shell.Current.GoToAsync("//lessons");
    }

    private async void OnBackToLessonsClicked(object sender, EventArgs e)
    {
        // Navigate back to lessons list
        await Shell.Current.GoToAsync("//lessons");
    }

    protected override bool OnBackButtonPressed()
    {
        // Override back button to go to lessons instead of lesson player
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//lessons");
        });
        
        return true;
    }
}