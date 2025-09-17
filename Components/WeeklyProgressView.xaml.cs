using LinguaLearn.Mobile.Models;

namespace LinguaLearn.Mobile.Components;

public partial class WeeklyProgressView : ContentView
{
    public static readonly BindableProperty WeeklyProgressProperty =
        BindableProperty.Create(nameof(WeeklyProgress), typeof(WeeklyProgress), typeof(WeeklyProgressView), null, propertyChanged: OnWeeklyProgressChanged);

    public WeeklyProgress WeeklyProgress
    {
        get => (WeeklyProgress)GetValue(WeeklyProgressProperty);
        set => SetValue(WeeklyProgressProperty, value);
    }

    public WeeklyProgressView()
    {
        InitializeComponent();
    }

    private static void OnWeeklyProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is WeeklyProgressView view && newValue is WeeklyProgress progress)
        {
            view.UpdateProgress(progress);
        }
    }

    private void UpdateProgress(WeeklyProgress progress)
    {
        if (progress == null) return;

        var progressPercentage = progress.Goal > 0 ? (double)progress.LessonsCompleted / progress.Goal : 0;
        progressPercentage = Math.Min(progressPercentage, 1.0); // Cap at 100%

        WeeklyProgressBar.Progress = progressPercentage;
        CircularProgress.Progress = progressPercentage;
    }

    private void UpdateProgressPath(double progress)
    {
        // For now, just use the regular progress bar
        // TODO: Implement custom circular progress drawing
        CircularProgress.Progress = progress;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is WeeklyProgress progress)
        {
            WeeklyProgress = progress;
        }
    }
}