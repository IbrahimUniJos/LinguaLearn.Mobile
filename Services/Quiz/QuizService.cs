using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Data;
using LinguaLearn.Mobile.Services.Activity;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Quizzes;

public class QuizService : IQuizService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IActivityService _activityService;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        IFirestoreRepository firestoreRepository,
        IActivityService activityService,
        ILogger<QuizService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _activityService = activityService;
        _logger = logger;
    }

    public async Task<ServiceResult<Quiz?>> GetQuizAsync(string quizId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(quizId))
                return ServiceResult<Quiz?>.Failure("Quiz ID is required");

            var result = await _firestoreRepository.GetDocumentAsync<Quiz>("quizzes", quizId, ct);
            return ServiceResult<Quiz?>.Success(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz {QuizId}", quizId);
            return ServiceResult<Quiz?>.Failure($"Error getting quiz: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Quiz>>> GetQuizzesByLessonAsync(string lessonId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetCollectionAsync<Quiz>("quizzes", ct);
            if (result.IsSuccess && result.Data != null)
            {
                var lessonQuizzes = result.Data
                    .Where(q => q.LessonId == lessonId && q.IsActive)
                    .OrderBy(q => q.Title)
                    .ToList();

                return ServiceResult<List<Quiz>>.Success(lessonQuizzes);
            }

            return ServiceResult<List<Quiz>>.Failure(result.ErrorMessage ?? "Failed to get quizzes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quizzes for lesson {LessonId}", lessonId);
            return ServiceResult<List<Quiz>>.Failure($"Error getting quizzes: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Quiz>> CreateQuizAsync(Quiz quiz, CancellationToken ct = default)
    {
        try
        {
            if (quiz == null)
                return ServiceResult<Quiz>.Failure("Quiz is required");

            quiz.Id = Guid.NewGuid().ToString();
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.UpdatedAt = DateTime.UtcNow;

            var result = await _firestoreRepository.SetDocumentAsync("quizzes", quiz.Id, quiz, ct);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Created quiz {QuizId}", quiz.Id);
                return ServiceResult<Quiz>.Success(quiz);
            }

            return ServiceResult<Quiz>.Failure(result.ErrorMessage ?? "Failed to create quiz");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quiz");
            return ServiceResult<Quiz>.Failure($"Error creating quiz: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Quiz>> UpdateQuizAsync(Quiz quiz, CancellationToken ct = default)
    {
        try
        {
            if (quiz == null || string.IsNullOrEmpty(quiz.Id))
                return ServiceResult<Quiz>.Failure("Quiz with valid ID is required");

            quiz.UpdatedAt = DateTime.UtcNow;

            var result = await _firestoreRepository.SetDocumentAsync("quizzes", quiz.Id, quiz, ct);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Updated quiz {QuizId}", quiz.Id);
                return ServiceResult<Quiz>.Success(quiz);
            }

            return ServiceResult<Quiz>.Failure(result.ErrorMessage ?? "Failed to update quiz");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quiz {QuizId}", quiz?.Id);
            return ServiceResult<Quiz>.Failure($"Error updating quiz: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteQuizAsync(string quizId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(quizId))
                return ServiceResult<bool>.Failure("Quiz ID is required");

            var result = await _firestoreRepository.DeleteDocumentAsync("quizzes", quizId, ct);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Deleted quiz {QuizId}", quizId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quiz {QuizId}", quizId);
            return ServiceResult<bool>.Failure($"Error deleting quiz: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizSession>> StartQuizSessionAsync(string userId, string quizId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(quizId))
                return ServiceResult<QuizSession>.Failure("User ID and Quiz ID are required");

            var quizResult = await GetQuizAsync(quizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<QuizSession>.Failure("Quiz not found");

            var quiz = quizResult.Data;
            var session = new QuizSession
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                QuizId = quizId,
                CurrentQuestionIndex = 0,
                TotalQuestions = quiz.Questions.Count,
                TimeRemaining = quiz.TimeLimit,
                StartedAt = DateTime.UtcNow
            };

            var result = await _firestoreRepository.SetDocumentAsync(
                $"users/{userId}/quizSessions", 
                session.Id, 
                session, 
                ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Started quiz session {SessionId} for user {UserId}", session.Id, userId);
                return ServiceResult<QuizSession>.Success(session);
            }

            return ServiceResult<QuizSession>.Failure(result.ErrorMessage ?? "Failed to start quiz session");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting quiz session for user {UserId} and quiz {QuizId}", userId, quizId);
            return ServiceResult<QuizSession>.Failure($"Error starting quiz session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizSession>> SubmitAnswerAsync(string sessionId, string questionId, List<string> answers, CancellationToken ct = default)
    {
        try
        {
            var sessionResult = await GetQuizSessionAsync(sessionId, ct);
            if (!sessionResult.IsSuccess || sessionResult.Data == null)
                return ServiceResult<QuizSession>.Failure("Quiz session not found");

            var session = sessionResult.Data;
            var quizResult = await GetQuizAsync(session.QuizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<QuizSession>.Failure("Quiz not found");

            var quiz = quizResult.Data;
            var question = quiz.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
                return ServiceResult<QuizSession>.Failure("Question not found");

            // Validate answer
            var isCorrect = QuizHelper.ValidateAnswer(question, answers);
            var pointsEarned = isCorrect ? question.Points : 0;

            // Create quiz answer
            var quizAnswer = new QuizAnswer
            {
                QuestionId = questionId,
                UserAnswers = answers,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                TimeSpentSeconds = 30, // This would be calculated from actual time
                AnsweredAt = DateTime.UtcNow
            };

            // Update session
            session.Answers.Add(quizAnswer);
            session.Score += pointsEarned;
            session.CurrentQuestionIndex++;

            // Save updated session
            var updateResult = await _firestoreRepository.SetDocumentAsync(
                $"users/{session.UserId}/quizSessions", 
                session.Id, 
                session, 
                ct);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Submitted answer for session {SessionId}, question {QuestionId}", sessionId, questionId);
                return ServiceResult<QuizSession>.Success(session);
            }

            return ServiceResult<QuizSession>.Failure(updateResult.ErrorMessage ?? "Failed to submit answer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer for session {SessionId}", sessionId);
            return ServiceResult<QuizSession>.Failure($"Error submitting answer: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizResult>> CompleteQuizSessionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var sessionResult = await GetQuizSessionAsync(sessionId, ct);
            if (!sessionResult.IsSuccess || sessionResult.Data == null)
                return ServiceResult<QuizResult>.Failure("Quiz session not found");

            var session = sessionResult.Data;
            var quizResult = await GetQuizAsync(session.QuizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<QuizResult>.Failure("Quiz not found");

            var quiz = quizResult.Data;
            var totalPoints = quiz.Questions.Sum(q => q.Points);
            var accuracy = QuizHelper.CalculateAccuracy(session.Answers);
            var timeSpent = (int)(DateTime.UtcNow - session.StartedAt).TotalSeconds;
            var isPassed = (session.Score * 100.0 / totalPoints) >= quiz.PassingScore;

            // Create quiz result
            var result = new QuizResult
            {
                Id = Guid.NewGuid().ToString(),
                UserId = session.UserId,
                QuizId = session.QuizId,
                SessionId = session.Id,
                Score = session.Score,
                TotalPoints = totalPoints,
                Accuracy = accuracy,
                TimeSpentSeconds = timeSpent,
                XPEarned = QuizHelper.CalculateXPForQuiz(new QuizResult 
                { 
                    Score = session.Score, 
                    TotalPoints = totalPoints, 
                    Accuracy = accuracy, 
                    TimeSpentSeconds = timeSpent 
                }),
                IsPassed = isPassed,
                Answers = session.Answers,
                CompletedAt = DateTime.UtcNow
            };

            // Mark session as completed
            session.IsCompleted = true;
            session.CompletedAt = DateTime.UtcNow;

            // Save both session and result
            var operations = new List<(string collection, string documentId, object document, BatchAction action)>
            {
                ($"users/{session.UserId}/quizSessions", session.Id, session, BatchAction.Set),
                ($"users/{session.UserId}/quizResults", result.Id, result, BatchAction.Set)
            };

            var batchResult = await _firestoreRepository.BatchWriteAsync(operations, ct);
            if (batchResult.IsSuccess)
            {
                // Record activity
                await _activityService.RecordQuizCompletedAsync(
                    session.UserId, 
                    quiz.Title, 
                    accuracy, 
                    result.XPEarned, 
                    ct);

                _logger.LogInformation("Completed quiz session {SessionId} with score {Score}/{TotalPoints}", 
                    sessionId, result.Score, result.TotalPoints);
                
                return ServiceResult<QuizResult>.Success(result);
            }

            return ServiceResult<QuizResult>.Failure(batchResult.ErrorMessage ?? "Failed to complete quiz session");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing quiz session {SessionId}", sessionId);
            return ServiceResult<QuizResult>.Failure($"Error completing quiz session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizSession?>> GetQuizSessionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
                return ServiceResult<QuizSession?>.Failure("Session ID is required");

            // We need to find the session across all users - this is a simplified approach
            // In a real implementation, you'd store the userId with the sessionId or use a different structure
            var result = await _firestoreRepository.GetDocumentAsync<QuizSession>("quizSessions", sessionId, ct);
            return ServiceResult<QuizSession?>.Success(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz session {SessionId}", sessionId);
            return ServiceResult<QuizSession?>.Failure($"Error getting quiz session: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizQuestion?>> GetNextQuestionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var sessionResult = await GetQuizSessionAsync(sessionId, ct);
            if (!sessionResult.IsSuccess || sessionResult.Data == null)
                return ServiceResult<QuizQuestion?>.Failure("Quiz session not found");

            var session = sessionResult.Data;
            var quizResult = await GetQuizAsync(session.QuizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<QuizQuestion?>.Failure("Quiz not found");

            var quiz = quizResult.Data;
            if (session.CurrentQuestionIndex >= quiz.Questions.Count)
                return ServiceResult<QuizQuestion?>.Success(null); // No more questions

            var nextQuestion = quiz.Questions
                .OrderBy(q => q.Order)
                .Skip(session.CurrentQuestionIndex)
                .FirstOrDefault();

            return ServiceResult<QuizQuestion?>.Success(nextQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next question for session {SessionId}", sessionId);
            return ServiceResult<QuizQuestion?>.Failure($"Error getting next question: {ex.Message}");
        }
    }

    public async Task<ServiceResult<QuizQuestion?>> GetCurrentQuestionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var sessionResult = await GetQuizSessionAsync(sessionId, ct);
            if (!sessionResult.IsSuccess || sessionResult.Data == null)
                return ServiceResult<QuizQuestion?>.Failure("Quiz session not found");

            var session = sessionResult.Data;
            var quizResult = await GetQuizAsync(session.QuizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<QuizQuestion?>.Failure("Quiz not found");

            var quiz = quizResult.Data;
            if (session.CurrentQuestionIndex >= quiz.Questions.Count)
                return ServiceResult<QuizQuestion?>.Success(null);

            var currentQuestion = quiz.Questions
                .OrderBy(q => q.Order)
                .Skip(session.CurrentQuestionIndex)
                .FirstOrDefault();

            return ServiceResult<QuizQuestion?>.Success(currentQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current question for session {SessionId}", sessionId);
            return ServiceResult<QuizQuestion?>.Failure($"Error getting current question: {ex.Message}");
        }
    }
    //TODO: Implement answer validation logic
    public async Task<ServiceResult<bool>> ValidateAnswerAsync(string questionId, List<string> userAnswers, CancellationToken ct = default)
    {
        try
        {
            // This would need to find the question across all quizzes
            // For now, return a placeholder implementation
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating answer for question {QuestionId}", questionId);
            return ServiceResult<bool>.Failure($"Error validating answer: {ex.Message}");
        }
    }
    //TODO: Implement adaptive difficulty logic
    public async Task<ServiceResult<double>> CalculateAdaptiveDifficultyAsync(string userId, string skillId, CancellationToken ct = default)
    {
        try
        {
            // Placeholder for adaptive difficulty calculation
            // This would analyze user's historical performance
            return ServiceResult<double>.Success(0.5); // Medium difficulty
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating adaptive difficulty for user {UserId}", userId);
            return ServiceResult<double>.Failure($"Error calculating adaptive difficulty: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<QuizQuestion>>> GetAdaptiveQuestionsAsync(string userId, string quizId, int count, CancellationToken ct = default)
    {
        try
        {
            var quizResult = await GetQuizAsync(quizId, ct);
            if (!quizResult.IsSuccess || quizResult.Data == null)
                return ServiceResult<List<QuizQuestion>>.Failure("Quiz not found");

            // For now, return random questions from the quiz
            var questions = quizResult.Data.Questions
                .OrderBy(q => Guid.NewGuid())
                .Take(count)
                .ToList();

            return ServiceResult<List<QuizQuestion>>.Success(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting adaptive questions for user {UserId}", userId);
            return ServiceResult<List<QuizQuestion>>.Failure($"Error getting adaptive questions: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<QuizResult>>> GetUserQuizHistoryAsync(string userId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetCollectionAsync<QuizResult>($"users/{userId}/quizResults", ct);
            if (result.IsSuccess && result.Data != null)
            {
                var history = result.Data
                    .OrderByDescending(r => r.CompletedAt)
                    .Take(limit)
                    .ToList();

                return ServiceResult<List<QuizResult>>.Success(history);
            }

            return ServiceResult<List<QuizResult>>.Success(new List<QuizResult>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz history for user {UserId}", userId);
            return ServiceResult<List<QuizResult>>.Failure($"Error getting quiz history: {ex.Message}");
        }
    }
    //TODO: Implement retrieval of specific quiz result
    public async Task<ServiceResult<QuizResult?>> GetQuizResultAsync(string resultId, CancellationToken ct = default)
    {
        try
        {
            // This would need to search across all users or have a different structure
            // For now, return placeholder
            return ServiceResult<QuizResult?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz result {ResultId}", resultId);
            return ServiceResult<QuizResult?>.Failure($"Error getting quiz result: {ex.Message}");
        }
    }

    public async Task<ServiceResult<double>> GetUserQuizAccuracyAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var historyResult = await GetUserQuizHistoryAsync(userId, 100, ct);
            if (historyResult.IsSuccess && historyResult.Data != null && historyResult.Data.Any())
            {
                var averageAccuracy = historyResult.Data.Average(r => r.Accuracy);
                return ServiceResult<double>.Success(averageAccuracy);
            }

            return ServiceResult<double>.Success(0.0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz accuracy for user {UserId}", userId);
            return ServiceResult<double>.Failure($"Error getting quiz accuracy: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetUserQuizCountAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var historyResult = await GetUserQuizHistoryAsync(userId, int.MaxValue, ct);
            if (historyResult.IsSuccess)
            {
                return ServiceResult<int>.Success(historyResult.Data?.Count ?? 0);
            }

            return ServiceResult<int>.Failure(historyResult.ErrorMessage ?? "Failed to get quiz count");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz count for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Error getting quiz count: {ex.Message}");
        }
    }
    //TODO: Implement retrieval of quiz statistics
    public async Task<ServiceResult<Dictionary<string, object>>> GetQuizStatisticsAsync(string quizId, CancellationToken ct = default)
    {
        try
        {
            // Placeholder for quiz statistics
            var stats = new Dictionary<string, object>
            {
                ["totalAttempts"] = 0,
                ["averageScore"] = 0.0,
                ["averageAccuracy"] = 0.0,
                ["passRate"] = 0.0
            };

            return ServiceResult<Dictionary<string, object>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quiz statistics for quiz {QuizId}", quizId);
            return ServiceResult<Dictionary<string, object>>.Failure($"Error getting quiz statistics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Dictionary<string, object>>> GetUserQuizStatisticsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var historyResult = await GetUserQuizHistoryAsync(userId, int.MaxValue, ct);
            var accuracyResult = await GetUserQuizAccuracyAsync(userId, ct);

            var stats = new Dictionary<string, object>
            {
                ["totalQuizzes"] = historyResult.Data?.Count ?? 0,
                ["averageAccuracy"] = accuracyResult.Data,
                ["totalXPEarned"] = historyResult.Data?.Sum(r => r.XPEarned) ?? 0,
                ["passedQuizzes"] = historyResult.Data?.Count(r => r.IsPassed) ?? 0
            };

            return ServiceResult<Dictionary<string, object>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user quiz statistics for user {UserId}", userId);
            return ServiceResult<Dictionary<string, object>>.Failure($"Error getting user quiz statistics: {ex.Message}");
        }
    }
}