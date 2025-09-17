using LinguaLearn.Mobile.Models;
using System.Windows.Input;

namespace LinguaLearn.Mobile.Components;

public partial class SectionContentView : ContentView
{
    public static readonly BindableProperty SectionProperty =
        BindableProperty.Create(nameof(Section), typeof(LessonSection), typeof(SectionContentView), null, propertyChanged: OnSectionChanged);

    public static readonly BindableProperty UserAnswerProperty =
        BindableProperty.Create(nameof(UserAnswer), typeof(string), typeof(SectionContentView), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty ShowFeedbackProperty =
        BindableProperty.Create(nameof(ShowFeedback), typeof(bool), typeof(SectionContentView), false, propertyChanged: OnShowFeedbackChanged);

    public static readonly BindableProperty IsCorrectProperty =
        BindableProperty.Create(nameof(IsCorrect), typeof(bool), typeof(SectionContentView), false, propertyChanged: OnIsCorrectChanged);

    public static readonly BindableProperty FeedbackTextProperty =
        BindableProperty.Create(nameof(FeedbackText), typeof(string), typeof(SectionContentView), string.Empty, propertyChanged: OnFeedbackTextChanged);

    public static readonly BindableProperty PlayAudioCommandProperty =
        BindableProperty.Create(nameof(PlayAudioCommand), typeof(ICommand), typeof(SectionContentView), null);

    public static readonly BindableProperty IsPlayingAudioProperty =
        BindableProperty.Create(nameof(IsPlayingAudio), typeof(bool), typeof(SectionContentView), false, propertyChanged: OnIsPlayingAudioChanged);

    public LessonSection? Section
    {
        get => (LessonSection?)GetValue(SectionProperty);
        set => SetValue(SectionProperty, value);
    }

    public string UserAnswer
    {
        get => (string)GetValue(UserAnswerProperty);
        set => SetValue(UserAnswerProperty, value);
    }

    public bool ShowFeedback
    {
        get => (bool)GetValue(ShowFeedbackProperty);
        set => SetValue(ShowFeedbackProperty, value);
    }

    public bool IsCorrect
    {
        get => (bool)GetValue(IsCorrectProperty);
        set => SetValue(IsCorrectProperty, value);
    }

    public string FeedbackText
    {
        get => (string)GetValue(FeedbackTextProperty);
        set => SetValue(FeedbackTextProperty, value);
    }

    public ICommand? PlayAudioCommand
    {
        get => (ICommand?)GetValue(PlayAudioCommandProperty);
        set => SetValue(PlayAudioCommandProperty, value);
    }

    public bool IsPlayingAudio
    {
        get => (bool)GetValue(IsPlayingAudioProperty);
        set => SetValue(IsPlayingAudioProperty, value);
    }

    public SectionContentView()
    {
        InitializeComponent();
        SetupBindings();
    }

    private void SetupBindings()
    {
        AnswerInput.SetBinding(Entry.TextProperty, new Binding(nameof(UserAnswer), source: this));
        PlayAudioButton.SetBinding(Button.CommandProperty, new Binding(nameof(PlayAudioCommand), source: this));
    }

    private static void OnSectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionContentView view && newValue is LessonSection section)
        {
            view.UpdateSectionContent(section);
        }
    }

    private static void OnShowFeedbackChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionContentView view)
        {
            view.UpdateFeedbackVisibility();
        }
    }

    private static void OnIsCorrectChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionContentView view)
        {
            view.UpdateFeedbackContent();
        }
    }

    private static void OnFeedbackTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionContentView view)
        {
            view.UpdateFeedbackMessage();
        }
    }
    
    private void UpdateFeedbackMessage()
    {
        if (FeedbackMessage != null)
        {
            FeedbackMessage.Text = this.FeedbackText;
        }
    }

    private static void OnIsPlayingAudioChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionContentView view)
        {
            view.UpdateAudioPlayingState();
        }
    }

    private void UpdateSectionContent(LessonSection section)
    {
        // Update badge text and color
        SectionTypeBadge.Text = GetSectionTypeDisplayName(section.Type);
        
        // Show/hide appropriate input sections based on type
        AudioSection.IsVisible = section.Type == "pronunciation";
        InputSection.IsVisible = RequiresTextInput(section.Type);
        MultipleChoiceSection.IsVisible = section.Type == "quiz" && HasMultipleChoiceOptions(section);
        
        // Update content
        ContentLabel.Text = section.Content;
        
        // Setup multiple choice options if needed
        if (section.Type == "quiz" && HasMultipleChoiceOptions(section))
        {
            SetupMultipleChoiceOptions(section);
        }
    }

    private void UpdateFeedbackVisibility()
    {
        FeedbackSection.IsVisible = ShowFeedback;
    }

    private void UpdateFeedbackContent()
    {
        if (ShowFeedback)
        {
            FeedbackIcon.Text = IsCorrect ? "✅" : "❌";
            FeedbackLabel.Text = IsCorrect ? "Correct!" : "Incorrect";
            FeedbackLabel.TextColor = IsCorrect ? Colors.Green : Colors.Red;
            FeedbackMessage.Text = this.FeedbackText;
        }
    }

    private void UpdateAudioPlayingState()
    {
        PlayAudioButton.IsEnabled = !IsPlayingAudio;
        AudioLoadingIndicator.IsRunning = IsPlayingAudio;
        AudioLoadingIndicator.IsVisible = IsPlayingAudio;
    }

    private string GetSectionTypeDisplayName(string sectionType)
    {
        return sectionType switch
        {
            "vocabulary" => "Vocabulary",
            "grammar" => "Grammar",
            "pronunciation" => "Pronunciation",
            "quiz" => "Quiz",
            "reading" => "Reading",
            "listening" => "Listening",
            _ => "Lesson"
        };
    }

    private bool RequiresTextInput(string sectionType)
    {
        return sectionType switch
        {
            "vocabulary" => true,
            "grammar" => true,
            _ => false
        };
    }

    private bool HasMultipleChoiceOptions(LessonSection section)
    {
        return section.Metadata.ContainsKey("options") && 
               section.Metadata["options"] is List<string> options && 
               options.Any();
    }

    private void SetupMultipleChoiceOptions(LessonSection section)
    {
        MultipleChoiceSection.Children.Clear();
        
        if (section.Metadata.TryGetValue("options", out var optionsObj) && 
            optionsObj is List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var optionButton = new Button
                {
                    Text = $"{(char)('A' + i)}. {option}",
                    BackgroundColor = Colors.Transparent,
                    TextColor = Color.FromArgb("#1C1B1F"),
                    BorderColor = Color.FromArgb("#79747E"),
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = new Thickness(16, 12),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CommandParameter = option
                };
                
                // Add tap handler for option selection
                optionButton.Clicked += (s, e) =>
                {
                    if (s is Button button && button.CommandParameter is string selectedOption)
                    {
                        UserAnswer = selectedOption;
                        HighlightSelectedOption(button);
                    }
                };
                
                MultipleChoiceSection.Children.Add(optionButton);
            }
        }
    }

    private void HighlightSelectedOption(Button selectedButton)
    {
        // Reset all buttons
        foreach (var child in MultipleChoiceSection.Children)
        {
            if (child is Button button)
            {
                button.BackgroundColor = Colors.Transparent;
                button.BorderColor = Color.FromArgb("#79747E");
            }
        }
        
        // Highlight selected button
        selectedButton.BackgroundColor = Color.FromArgb("#6750A4");
        selectedButton.TextColor = Colors.White;
        selectedButton.BorderColor = Color.FromArgb("#6750A4");
    }
}