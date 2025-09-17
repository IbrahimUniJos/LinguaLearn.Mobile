using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Quizzes;

/// <summary>
/// Service for managing quizzes and quiz sessions
/// </summary>
public interface IQuizService
{
    // Quiz Operations
    Task<ServiceResult<Quiz?>> GetQuizAsync(string quizId, CancellationToken ct = default);
    Task<ServiceResult<List<Quiz>>> GetQuizzesByLessonAsync(string lessonId, CancellationToken ct = default);
    Task<ServiceResult<Quiz>> CreateQuizAsync(Quiz quiz, CancellationToken ct = default);
    Task<ServiceResult<Quiz>> UpdateQuizAsync(Quiz quiz, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteQuizAsync(string quizId, CancellationToken ct = default);
    
    // Quiz Session Management
    Task<ServiceResult<QuizSession>> StartQuizSessionAsync(string userId, string quizId, CancellationToken ct = default);
    Task<ServiceResult<QuizSession>> SubmitAnswerAsync(string sessionId, string questionId, List<string> answers, CancellationToken ct = default);
    Task<ServiceResult<QuizResult>> CompleteQuizSessionAsync(string sessionId, CancellationToken ct = default);
    Task<ServiceResult<QuizSession?>> GetQuizSessionAsync(string sessionId, CancellationToken ct = default);
    
    // Question Management
    Task<ServiceResult<QuizQuestion?>> GetNextQuestionAsync(string sessionId, CancellationToken ct = default);
    Task<ServiceResult<QuizQuestion?>> GetCurrentQuestionAsync(string sessionId, CancellationToken ct = default);
    Task<ServiceResult<bool>> ValidateAnswerAsync(string questionId, List<string> userAnswers, CancellationToken ct = default);
    
    // Adaptive Engine
    Task<ServiceResult<double>> CalculateAdaptiveDifficultyAsync(string userId, string skillId, CancellationToken ct = default);
    Task<ServiceResult<List<QuizQuestion>>> GetAdaptiveQuestionsAsync(string userId, string quizId, int count, CancellationToken ct = default);
    
    // History and Analytics
    Task<ServiceResult<List<QuizResult>>> GetUserQuizHistoryAsync(string userId, int limit = 10, CancellationToken ct = default);
    Task<ServiceResult<QuizResult?>> GetQuizResultAsync(string resultId, CancellationToken ct = default);
    Task<ServiceResult<double>> GetUserQuizAccuracyAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> GetUserQuizCountAsync(string userId, CancellationToken ct = default);
    
    // Statistics
    Task<ServiceResult<Dictionary<string, object>>> GetQuizStatisticsAsync(string quizId, CancellationToken ct = default);
    Task<ServiceResult<Dictionary<string, object>>> GetUserQuizStatisticsAsync(string userId, CancellationToken ct = default);
}