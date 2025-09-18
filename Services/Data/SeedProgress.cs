namespace LinguaLearn.Mobile.Services.Data;

public sealed record SeedProgress(
    int TotalLessons,
    int ProcessedLessons,
    int TotalQuizzes,
    int ProcessedQuizzes,
    int TotalQuestions,
    int ProcessedQuestions,
    string Phase,
    string? Message)
{
    public double Percent =>
        TotalOperations == 0 ? 0 : (double)ProcessedOperations / TotalOperations;

    public int TotalOperations => TotalLessons + TotalQuizzes + TotalQuestions;
    public int ProcessedOperations => ProcessedLessons + ProcessedQuizzes + ProcessedQuestions;
}
