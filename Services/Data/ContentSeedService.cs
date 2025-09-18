using System.Text.Json;
using Google.Cloud.Firestore;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Data;

/// <summary>
/// Seeds lessons, quizzes, and questions from a bundled JSON asset into Firestore.
/// JSON shape must match Resources/Raw/seed-lessons-quizzes.json.
/// </summary>
public class ContentSeedService
{
    private readonly IFirestoreRepository _repo;
    private readonly ILogger<ContentSeedService> _logger;

    public ContentSeedService(IFirestoreRepository repo, ILogger<ContentSeedService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Seed content from a MAUI asset JSON file into Firestore collections:
    /// lessons/{lessonId}, lessons/{lessonId}/quizzes/{quizId}, lessons/{lessonId}/quizzes/{quizId}/questions/{questionId}
    /// </summary>
    /// <param name="assetFileName">The MAUI asset file name. Default: seed-lessons-quizzes.json</param>
    /// <param name="ct">Cancellation token</param>
    public async Task SeedFromAssetAsync(string assetFileName = "seed-lessons-quizzes.json", CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            await SeedFromJsonAsync(json, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed content from asset {Asset}", assetFileName);
            throw;
        }
    }

    /// <summary>
    /// Progress-capable overload for seeding from an asset.
    /// </summary>
    public async Task SeedFromAssetAsync(string assetFileName, IProgress<SeedProgress> progress, CancellationToken ct = default)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            await SeedFromJsonAsync(json, progress, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed content from asset {Asset}", assetFileName);
            progress.Report(new SeedProgress(0, 0, 0, 0, 0, 0, "Error", ex.Message));
            throw;
        }
    }

    /// <summary>
    /// Seed content from a JSON string
    /// </summary>
    public async Task SeedFromJsonAsync(string json, CancellationToken ct = default)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        if (!document.RootElement.TryGetProperty("lessons", out var lessonsEl) || lessonsEl.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Invalid seed JSON: missing 'lessons' array");
        }

        int lessonCount = 0, quizCount = 0, questionCount = 0;

        foreach (var lessonEl in lessonsEl.EnumerateArray())
        {
            // Convert lesson object to dictionary and extract child arrays
            var lessonDict = ToDictionary(lessonEl);
            if (!lessonDict.TryGetValue("id", out var idObj) || idObj is not string lessonId || string.IsNullOrWhiteSpace(lessonId))
            {
                throw new InvalidOperationException("Each lesson must have a non-empty 'id'");
            }

            // Extract and remove nested arrays before writing the lesson doc
            var quizzesArray = ExtractArray(lessonDict, "quizzes");

            // Write lesson document
            await _repo.SetDocumentAsync("lessons", lessonId, lessonDict, ct);
            lessonCount++;

            // Quizzes
            if (quizzesArray != null)
            {
                foreach (var quizEl in quizzesArray)
                {
                    var quizDict = ToDictionary(quizEl);

                    if (!quizDict.TryGetValue("id", out var qidObj) || qidObj is not string quizId || string.IsNullOrWhiteSpace(quizId))
                    {
                        throw new InvalidOperationException($"Quiz under lesson '{lessonId}' is missing 'id'");
                    }

                    var questionsArray = ExtractArray(quizDict, "questions");

                    // Write quiz document under lesson
                    var quizCollectionPath = $"lessons/{lessonId}/quizzes";
                    await _repo.SetDocumentAsync(quizCollectionPath, quizId, quizDict, ct);
                    quizCount++;

                    // Questions
                    if (questionsArray != null)
                    {
                        foreach (var questionEl in questionsArray)
                        {
                            var qDict = ToDictionary(questionEl);
                            if (!qDict.TryGetValue("id", out var qqidObj) || qqidObj is not string questionId || string.IsNullOrWhiteSpace(questionId))
                            {
                                throw new InvalidOperationException($"Question under lesson '{lessonId}' quiz '{quizId}' is missing 'id'");
                            }

                            var questionCollectionPath = $"lessons/{lessonId}/quizzes/{quizId}/questions";
                            await _repo.SetDocumentAsync(questionCollectionPath, questionId, qDict, ct);
                            questionCount++;
                        }
                    }
                }
            }
        }

        _logger.LogInformation("Seed completed: {Lessons} lessons, {Quizzes} quizzes, {Questions} questions", lessonCount, quizCount, questionCount);
    }

    /// <summary>
    /// Progress-capable overload for seeding from a JSON string.
    /// </summary>
    public async Task SeedFromJsonAsync(string json, IProgress<SeedProgress> progress, CancellationToken ct = default)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        if (!document.RootElement.TryGetProperty("lessons", out var lessonsEl) || lessonsEl.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Invalid seed JSON: missing 'lessons' array");
        }

        // Compute totals for progress
        int totalLessons = 0, totalQuizzes = 0, totalQuestions = 0;
        foreach (var l in lessonsEl.EnumerateArray())
        {
            totalLessons++;
            if (l.TryGetProperty("quizzes", out var qs) && qs.ValueKind == JsonValueKind.Array)
            {
                foreach (var q in qs.EnumerateArray())
                {
                    totalQuizzes++;
                    if (q.TryGetProperty("questions", out var qq) && qq.ValueKind == JsonValueKind.Array)
                    {
                        totalQuestions += qq.GetArrayLength();
                    }
                }
            }
        }

        int processedLessons = 0, processedQuizzes = 0, processedQuestions = 0;
        progress.Report(new SeedProgress(totalLessons, processedLessons, totalQuizzes, processedQuizzes, totalQuestions, processedQuestions, "Preparing", "Starting content seed..."));

        foreach (var lessonEl in lessonsEl.EnumerateArray())
        {
            var lessonDict = ToDictionary(lessonEl);
            if (!lessonDict.TryGetValue("id", out var idObj) || idObj is not string lessonId || string.IsNullOrWhiteSpace(lessonId))
            {
                throw new InvalidOperationException("Each lesson must have a non-empty 'id'");
            }

            var quizzesArray = ExtractArray(lessonDict, "quizzes");

            await _repo.SetDocumentAsync("lessons", lessonId, lessonDict, ct);
            processedLessons++;
            progress.Report(new SeedProgress(totalLessons, processedLessons, totalQuizzes, processedQuizzes, totalQuestions, processedQuestions, "Lesson", $"Wrote lesson {lessonId}"));

            if (quizzesArray != null)
            {
                foreach (var quizEl in quizzesArray)
                {
                    var quizDict = ToDictionary(quizEl);

                    if (!quizDict.TryGetValue("id", out var qidObj) || qidObj is not string quizId || string.IsNullOrWhiteSpace(quizId))
                    {
                        throw new InvalidOperationException($"Quiz under lesson '{lessonId}' is missing 'id'");
                    }

                    var questionsArray = ExtractArray(quizDict, "questions");

                    var quizCollectionPath = $"lessons/{lessonId}/quizzes";
                    await _repo.SetDocumentAsync(quizCollectionPath, quizId, quizDict, ct);
                    processedQuizzes++;
                    progress.Report(new SeedProgress(totalLessons, processedLessons, totalQuizzes, processedQuizzes, totalQuestions, processedQuestions, "Quiz", $"Wrote quiz {quizId}"));

                    if (questionsArray != null)
                    {
                        foreach (var questionEl in questionsArray)
                        {
                            var qDict = ToDictionary(questionEl);
                            if (!qDict.TryGetValue("id", out var qqidObj) || qqidObj is not string questionId || string.IsNullOrWhiteSpace(questionId))
                            {
                                throw new InvalidOperationException($"Question under lesson '{lessonId}' quiz '{quizId}' is missing 'id'");
                            }

                            var questionCollectionPath = $"lessons/{lessonId}/quizzes/{quizId}/questions";
                            await _repo.SetDocumentAsync(questionCollectionPath, questionId, qDict, ct);
                            processedQuestions++;
                            if (processedQuestions % 3 == 0 || processedQuestions == totalQuestions)
                            {
                                progress.Report(new SeedProgress(totalLessons, processedLessons, totalQuizzes, processedQuizzes, totalQuestions, processedQuestions, "Question", $"Wrote question {questionId}"));
                            }
                        }
                    }
                }
            }
        }

        progress.Report(new SeedProgress(totalLessons, processedLessons, totalQuizzes, processedQuizzes, totalQuestions, processedQuestions, "Completed", "Seeding finished."));
        _logger.LogInformation("Seed completed: {Lessons} lessons, {Quizzes} quizzes, {Questions} questions", processedLessons, processedQuizzes, processedQuestions);
    }

    private static List<JsonElement>? ExtractArray(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        dict.Remove(key);

        if (value is List<object> list)
        {
            // Convert List<object> back to List<JsonElement> via re-parse to keep uniform handling
            var result = new List<JsonElement>(list.Count);
            foreach (var item in list)
            {
                // Serialize the item then parse back to JsonElement for ToDictionary processing
                var json = System.Text.Json.JsonSerializer.Serialize(item);
                using var doc = JsonDocument.Parse(json);
                result.Add(doc.RootElement.Clone());
            }
            return result;
        }

        return null;
    }

    /// <summary>
    /// Recursively converts a JsonElement into a Dictionary/List/primitive suitable for Firestore SDK.
    /// Replaces literal string "SERVER_TIMESTAMP" with FieldValue.ServerTimestamp.
    /// </summary>
    private static Dictionary<string, object> ToDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Expected JSON object", nameof(element));

        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = ConvertJsonValue(prop.Value);
        }

        return dict;
    }

    private static object? ConvertJsonValue(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in value.EnumerateObject())
                {
                    map[p.Name] = ConvertJsonValue(p.Value);
                }
                return map;
            }
            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in value.EnumerateArray())
                {
                    list.Add(ConvertJsonValue(item));
                }
                return list;
            }
            case JsonValueKind.String:
            {
                var s = value.GetString();
                if (string.Equals(s, "SERVER_TIMESTAMP", StringComparison.Ordinal))
                {
                    return FieldValue.ServerTimestamp;
                }
                return s;
            }
            case JsonValueKind.Number:
            {
                if (value.TryGetInt64(out var l)) return l;
                if (value.TryGetDouble(out var d)) return d;
                return null;
            }
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return null;
        }
    }
}
