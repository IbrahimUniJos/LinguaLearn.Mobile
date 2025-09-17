namespace LinguaLearn.Mobile.Components;

public partial class LessonProgressIndicator : ContentView
{
    public static readonly BindableProperty CurrentSectionProperty =
        BindableProperty.Create(nameof(CurrentSection), typeof(int), typeof(LessonProgressIndicator), 1, propertyChanged: OnProgressChanged);

    public static readonly BindableProperty TotalSectionsProperty =
        BindableProperty.Create(nameof(TotalSections), typeof(int), typeof(LessonProgressIndicator), 1, propertyChanged: OnProgressChanged);

    public static readonly BindableProperty XPEarnedProperty =
        BindableProperty.Create(nameof(XPEarned), typeof(int), typeof(LessonProgressIndicator), 0, propertyChanged: OnXPChanged);

    public static readonly BindableProperty SectionTitleProperty =
        BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(LessonProgressIndicator), "Section", propertyChanged: OnSectionTitleChanged);

    public int CurrentSection
    {
        get => (int)GetValue(CurrentSectionProperty);
        set => SetValue(CurrentSectionProperty, value);
    }

    public int TotalSections
    {
        get => (int)GetValue(TotalSectionsProperty);
        set => SetValue(TotalSectionsProperty, value);
    }

    public int XPEarned
    {
        get => (int)GetValue(XPEarnedProperty);
        set => SetValue(XPEarnedProperty, value);
    }

    public string SectionTitle
    {
        get => (string)GetValue(SectionTitleProperty);
        set => SetValue(SectionTitleProperty, value);
    }

    public LessonProgressIndicator()
    {
        InitializeComponent();
        UpdateProgress();
    }

    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LessonProgressIndicator indicator)
        {
            indicator.UpdateProgress();
        }
    }

    private static void OnXPChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LessonProgressIndicator indicator)
        {
            indicator.UpdateXP();
        }
    }

    private static void OnSectionTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LessonProgressIndicator indicator)
        {
            indicator.UpdateSectionTitle();
        }
    }

    private void UpdateProgress()
    {
        if (TotalSections > 0)
        {
            var progress = (double)CurrentSection / TotalSections;
            MainProgressBar.Progress = progress;
            ProgressLabel.Text = $"{CurrentSection} of {TotalSections}";
        }
    }

    private void UpdateXP()
    {
        XPLabel.Text = XPEarned > 0 ? $"+{XPEarned} XP" : "+0 XP";
    }

    private void UpdateSectionTitle()
    {
        CurrentSectionLabel.Text = SectionTitle ?? "Section";
    }
}