using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Services.Quizzes;
using LinguaLearn.Mobile.Services.User;
using LinguaLearn.Mobile.Services.Progress;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.ViewModels;

/// <summary>
/// ViewModel for quiz functionality
/// </summary>
public partial class QuizViewModel : ObservableObject, IDisposable
{
    private readonly IQuizService _quizService;
    private readonly IUserService _userService;
    private readonly IProgressService _progressService;
    private readonly ILogger<QuizViewModel> _logger;

    [ObservableProperty]
    private Quiz? _currentQuiz;

    [ObservableProperty]
    private QuizSession? _currentSession;

    [ObservableProperty]
    private QuizQuestion? _currentQuestion;

    [ObservableProperty]
    private int _currentQuestionIndex;

    [ObservableProperty]
    private int _totalQuestions;

    [ObservableProperty]
    private TimeSpan _timeRemaining;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _xpEarned;

    [ObservableProperty]
    private string? _userAnswer;

    [ObservableProperty]
    private int _selectedOption = -1;

    [ObservableProperty]
    private bool _showFeedback;

    [ObservableProperty]
    private bool _isAnswerCorrect;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isQuizCompleted;

    [ObservableProperty]
    private QuizResult? _quizResult;

    public ObservableCollection<string> QuestionOptions { get; } = new();
    public ObservableCollection<QuizAnswer> UserAnswers { get; } = new();

    private Timer? _quizTimer;
    private DateTime _questionStartTime;

    public QuizViewModel(
        IQuizService quizService,
        IUserService userService,
        IProgressService progressService,
        ILogger<QuizViewModel> logger)
    {
        _quizService = quizService;
        _userService = userService;
        _progressService = progressService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize quiz with the given quiz ID
    /// </summary>
    public async Task InitializeAsync(string quizId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Get quiz data
            var quizResult = await _quizService.GetQuizAsync(quizId);
            if (!quizResult.IsSuccess || quizResult.Data == null)
            {
                ErrorMessage = quizResult.ErrorMessage ?? "Failed to load quiz";
                return;
            }

            CurrentQuiz = quizResult.Data;
            TotalQuestions = CurrentQuiz.Questions.Count;

            // Start quiz session
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User not authenticated";
                return;
            }

            var sessionResult = await _quizService.StartQuizSessionAsync(userId, quizId);
            if (!sessionResult.IsSuccess || sessionResult.Data == null)
            {
                ErrorMessage = sessionResult.ErrorMessage ?? "Failed to start quiz session";
                return;
            }

            CurrentSession = sessionResult.Data;
            TimeRemaining = TimeSpan.FromSeconds(CurrentQuiz.TimeLimit);

            // Load first question
            await LoadCurrentQuestionAsync();

            // Start timer
            StartQuizTimer();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing quiz {QuizId}", quizId);
            ErrorMessage = $"Error initializing quiz: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load the current question based on session state
    /// </summary>
    private async Task LoadCurrentQuestionAsync()
    {
        try
        {
            if (CurrentSession == null)
                return;

            var questionResult = await _quizService.GetCurrentQuestionAsync(CurrentSession.Id);
            if (questionResult.IsSuccess && questionResult.Data != null)
            {
                CurrentQuestion = questionResult.Data;
                CurrentQuestionIndex = CurrentSession.CurrentQuestionIndex + 1; // 1-based for display

                // Load question options
                QuestionOptions.Clear();
                foreach (var option in CurrentQuestion.Options)
                {
                    QuestionOptions.Add(option);
                }

                // Reset question state
                UserAnswer = string.Empty;
                SelectedOption = -1;
                ShowFeedback = false;
                IsAnswerCorrect = false;
                _questionStartTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading current question");
            ErrorMessage = $"Error loading question: {ex.Message}";
        }
    }

    /// <summary>
    /// Select an option for multiple choice questions
    /// </summary>
    [RelayCommand]
    private void SelectOption(int optionIndex)
    {
        if (ShowFeedback || optionIndex < 0 || optionIndex >= QuestionOptions.Count)
            return;

        SelectedOption = optionIndex;
        UserAnswer = QuestionOptions[optionIndex];
    }

    /// <summary>
    /// Submit the current answer
    /// </summary>
    [RelayCommand]
    private async Task SubmitAnswerAsync()
    {
        try
        {
            if (CurrentSession == null || CurrentQuestion == null || ShowFeedback)
                return;

            if (string.IsNullOrWhiteSpace(UserAnswer))
            {
                ErrorMessage = "Please provide an answer";
                return;
            }

            IsLoading = true;

            // Calculate time spent on this question
            var timeSpent = (int)(DateTime.UtcNow - _questionStartTime).TotalSeconds;

            // Submit answer
            var answers = new List<string> { UserAnswer };
            var submitResult = await _quizService.SubmitAnswerAsync(
                CurrentSession.Id, 
                CurrentQuestion.Id, 
                answers);

            if (!submitResult.IsSuccess || submitResult.Data == null)
            {
                ErrorMessage = submitResult.ErrorMessage ?? "Failed to submit answer";
                return;
            }

            CurrentSession = submitResult.Data;

            // Validate answer
            var validationResult = await _quizService.ValidateAnswerAsync(CurrentQuestion.Id, answers);
            IsAnswerCorrect = validationResult.IsSuccess && validationResult.Data;

            // Update score and XP
            if (IsAnswerCorrect)
            {
                Score += CurrentQuestion.Points;
                XpEarned += CurrentQuestion.Points * 2; // 2 XP per point
            }

            // Record the answer
            var quizAnswer = new QuizAnswer
            {
                QuestionId = CurrentQuestion.Id,
                UserAnswers = answers,
                IsCorrect = IsAnswerCorrect,
                PointsEarned = IsAnswerCorrect ? CurrentQuestion.Points : 0,
                TimeSpentSeconds = timeSpent
            };

            UserAnswers.Add(quizAnswer);

            // Show feedback
            ShowFeedback = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer");
            ErrorMessage = $"Error submitting answer: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Move to the next question or complete quiz
    /// </summary>
    [RelayCommand]
    private async Task NextQuestionAsync()
    {
        try
        {
            if (CurrentSession == null)
                return;

            // Check if this was the last question
            if (CurrentSession.CurrentQuestionIndex >= TotalQuestions - 1)
            {
                await CompleteQuizAsync();
                return;
            }

            // Load next question
            ShowFeedback = false;
            await LoadCurrentQuestionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving to next question");
            ErrorMessage = $"Error loading next question: {ex.Message}";
        }
    }

    /// <summary>
    /// Complete the quiz and show results
    /// </summary>
    private async Task CompleteQuizAsync()
    {
        try
        {
            if (CurrentSession == null)
                return;

            IsLoading = true;

            // Stop timer
            StopQuizTimer();

            // Complete quiz session
            var completeResult = await _quizService.CompleteQuizSessionAsync(CurrentSession.Id);
            if (!completeResult.IsSuccess || completeResult.Data == null)
            {
                ErrorMessage = completeResult.ErrorMessage ?? "Failed to complete quiz";
                return;
            }

            QuizResult = completeResult.Data;
            IsQuizCompleted = true;

            // Record progress
            var userId = await _userService.GetCurrentUserIdAsync();
            if (!string.IsNullOrEmpty(userId) && CurrentQuiz != null)
            {
                var progressRecord = new ProgressRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    LessonId = CurrentQuiz.LessonId,
                    SectionId = CurrentQuiz.Id,
                    Score = QuizResult.Accuracy,
                    Accuracy = QuizResult.Accuracy,
                    TimeSpentSeconds = QuizResult.TimeSpentSeconds,
                    XPEarned = QuizResult.XPEarned,
                    IsCompleted = QuizResult.IsPassed,
                    Metadata = new Dictionary<string, object>
                    {
                        ["quizScore"] = QuizResult.Score,
                        ["totalPoints"] = QuizResult.TotalPoints,
                        ["isPassed"] = QuizResult.IsPassed
                    }
                };

                await _progressService.RecordProgressAsync(progressRecord);
            }

            _logger.LogInformation("Quiz completed. Score: {Score}/{TotalPoints}, Accuracy: {Accuracy:P2}", 
                QuizResult.Score, QuizResult.TotalPoints, QuizResult.Accuracy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing quiz");
            ErrorMessage = $"Error completing quiz: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Restart the quiz
    /// </summary>
    [RelayCommand]
    private async Task RestartQuizAsync()
    {
        try
        {
            if (CurrentQuiz == null)
                return;

            // Reset state
            IsQuizCompleted = false;
            QuizResult = null;
            Score = 0;
            XpEarned = 0;
            UserAnswers.Clear();
            ShowFeedback = false;
            ErrorMessage = null;

            // Reinitialize
            await InitializeAsync(CurrentQuiz.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting quiz");
            ErrorMessage = $"Error restarting quiz: {ex.Message}";
        }
    }

    /// <summary>
    /// Exit the quiz
    /// </summary>
    [RelayCommand]
    private async Task ExitQuizAsync()
    {
        try
        {
            StopQuizTimer();
            
            // Navigate back
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exiting quiz");
        }
    }

    /// <summary>
    /// Start the quiz timer
    /// </summary>
    private void StartQuizTimer()
    {
        StopQuizTimer();

        _quizTimer = new Timer(UpdateTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Stop the quiz timer
    /// </summary>
    private void StopQuizTimer()
    {
        _quizTimer?.Dispose();
        _quizTimer = null;
    }

    /// <summary>
    /// Update timer callback
    /// </summary>
    private void UpdateTimer(object? state)
    {
        if (TimeRemaining > TimeSpan.Zero)
        {
            TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
        }
        else
        {
            // Time's up - auto-complete quiz
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CompleteQuizAsync();
            });
        }
    }

    /// <summary>
    /// Get progress percentage
    /// </summary>
    public double ProgressPercentage => TotalQuestions > 0 ? (double)CurrentQuestionIndex / TotalQuestions * 100 : 0;

    /// <summary>
    /// Get formatted time remaining
    /// </summary>
    public string FormattedTimeRemaining => $"{TimeRemaining:mm\\:ss}";

    /// <summary>
    /// Check if answer can be submitted
    /// </summary>
    public bool CanSubmitAnswer => !ShowFeedback && !string.IsNullOrWhiteSpace(UserAnswer);

    /// <summary>
    /// Get action button text
    /// </summary>
    public string ActionButtonText => ShowFeedback 
        ? (CurrentQuestionIndex >= TotalQuestions ? "Complete Quiz" : "Next Question")
        : "Submit Answer";

    public void Dispose()
    {
        StopQuizTimer();
    }
}