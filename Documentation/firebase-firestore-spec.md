# Firebase Firestore Specification - LinguaLearn MAUI

## Overview
This specification details the implementation of Firebase Firestore using the official Google.Cloud.Firestore NuGet package for the LinguaLearn MAUI application. This approach provides robust, type-safe access to Firestore with built-in offline support and integrates seamlessly with the Firebase Authentication service.

## Architecture

### Core Components
- **IFirestoreRepository**: Main Firestore repository interface
- **FirestoreRepository**: Concrete implementation using Google.Cloud.Firestore
- **FirestoreDb**: Direct Google Cloud Firestore database instance
- **FirestoreModels**: Domain models with Firestore attributes
- **DateTimeToTimestampConverter**: Custom converter for DateTime to Firestore Timestamp

## Google.Cloud.Firestore Integration

### NuGet Packages Required
```xml
<PackageReference Include="Google.Cloud.Firestore" Version="3.7.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
```

### Firestore Setup with Service Account JSON

The correct approach uses the Firebase service account JSON file from the app package:

```csharp
public class DateTimeToTimestampConverter : IFirestoreConverter<DateTime>
{
    public DateTime FromFirestore(object value)
    {
        return ((Timestamp)value).ToDateTime();
    }

    public object ToFirestore(DateTime value)
    {
        return Timestamp.FromDateTime(value.ToUniversalTime());
    }
}

public static class FirestoreServiceCollectionExtensions
{
    private const string DefaultProjectId = "your-project-id";

    public static IServiceCollection AddFirestore(
        this IServiceCollection services,
        string? projectId = DefaultProjectId,
        string credentialsFileName = "firebase-adminsdk.json",
        bool useEmulator = false)
    {
        services.AddSingleton<FirestoreDb>(sp =>
        {
            // Load credentials file from app package
            string jsonCredentials;
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(credentialsFileName).GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                jsonCredentials = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Firestore credentials file '{credentialsFileName}': {ex.Message}", ex);
            }

            var converterRegistry = new ConverterRegistry
            {
                new DateTimeToTimestampConverter()
            };

            var effectiveProjectId = projectId ?? DefaultProjectId;
            var builder = new FirestoreDbBuilder
            {
                ProjectId = effectiveProjectId,
                ConverterRegistry = converterRegistry,
                JsonCredentials = jsonCredentials
            };

            if (useEmulator)
            {
                builder.EmulatorDetection = EmulatorDetection.EmulatorOnly;
                Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", "localhost:8080");
            }

            return builder.Build();
        });

        return services;
    }
}
```

## Data Models

### Domain Models with Firestore Attributes
Using Google.Cloud.Firestore attributes for automatic serialization:

### User Models
```csharp
[FirestoreData]
public class User
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [FirestoreProperty("xp")]
    public int XP { get; set; }

    [FirestoreProperty("level")]
    public int Level { get; set; }

    [FirestoreProperty("currentStreak")]
    public int CurrentStreak { get; set; }

    [FirestoreProperty("longestStreak")]
    public int LongestStreak { get; set; }

    [FirestoreProperty("lastActivityDate")]
    public DateTime LastActivityDate { get; set; }

    [FirestoreProperty("earnedBadges")]
    public List<string> EarnedBadges { get; set; } = new();

    [FirestoreProperty("preferences")]
    public UserPreferences Preferences { get; set; } = new();

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

[FirestoreData]
public class UserPreferences
{
    [FirestoreProperty("preferredLanguage")]
    public string PreferredLanguage { get; set; } = "en";

    [FirestoreProperty("soundEnabled")]
    public bool SoundEnabled { get; set; } = true;

    [FirestoreProperty("notificationsEnabled")]
    public bool NotificationsEnabled { get; set; } = true;

    [FirestoreProperty("difficultyLevel")]
    public string DifficultyLevel { get; set; } = "beginner";

    [FirestoreProperty("dailyGoalMinutes")]
    public int DailyGoalMinutes { get; set; } = 15;
}
```

### Lesson Models
```csharp
[FirestoreData]
public class Lesson
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("language")]
    public string Language { get; set; } = string.Empty;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [FirestoreProperty("order")]
    public int Order { get; set; }

    [FirestoreProperty("prerequisites")]
    public List<string> Prerequisites { get; set; } = new();

    [FirestoreProperty("sections")]
    public List<LessonSection> Sections { get; set; } = new();

    [FirestoreProperty("estimatedDurationMinutes")]
    public int EstimatedDurationMinutes { get; set; }

    [FirestoreProperty("xpReward")]
    public int XPReward { get; set; }

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [FirestoreProperty("createdAt")]
    public Timestamp CreatedAt { get; set; }

    [FirestoreProperty("updatedAt")]
    public Timestamp UpdatedAt { get; set; }
}

[FirestoreData]
public class LessonSection
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "vocabulary", "grammar", "pronunciation", "quiz"

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("content")]
    public string Content { get; set; } = string.Empty;

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("order")]
    public int Order { get; set; }
}
```

### Progress Models
```csharp
[FirestoreData]
public class ProgressRecord
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [FirestoreProperty("score")]
    public double Score { get; set; }

    [FirestoreProperty("accuracy")]
    public double Accuracy { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("xpEarned")]
    public int XPEarned { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("completedAt")]
    public Timestamp CompletedAt { get; set; }
}

[FirestoreData]
public class UserProgress
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("overallScore")]
    public double OverallScore { get; set; }

    [FirestoreProperty("overallAccuracy")]
    public double OverallAccuracy { get; set; }

    [FirestoreProperty("totalTimeSpent")]
    public int TotalTimeSpent { get; set; }

    [FirestoreProperty("totalXPEarned")]
    public int TotalXPEarned { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("firstAttempt")]
    public Timestamp FirstAttempt { get; set; }

    [FirestoreProperty("lastAttempt")]
    public Timestamp LastAttempt { get; set; }

    [FirestoreProperty("attemptCount")]
    public int AttemptCount { get; set; }
}
```

### Quiz Models
```csharp
[FirestoreData]
public class Quiz
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("questions")]
    public List<QuizQuestion> Questions { get; set; } = new();

    [FirestoreProperty("timeLimit")]
    public int TimeLimit { get; set; }

    [FirestoreProperty("passingScore")]
    public int PassingScore { get; set; }

    [FirestoreProperty("adaptiveSettings")]
    public AdaptiveConfig AdaptiveSettings { get; set; } = new();

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;
}

[FirestoreData]
public class QuizQuestion
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "multiple_choice", "fill_blank", "matching", etc.

    [FirestoreProperty("question")]
    public string Question { get; set; } = string.Empty;

    [FirestoreProperty("options")]
    public List<string> Options { get; set; } = new();

    [FirestoreProperty("correctAnswers")]
    public List<string> CorrectAnswers { get; set; } = new();

    [FirestoreProperty("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [FirestoreProperty("points")]
    public int Points { get; set; } = 1;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = "medium";

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

[FirestoreData]
public class AdaptiveConfig
{
    [FirestoreProperty("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    [FirestoreProperty("difficultyAdjustmentFactor")]
    public double DifficultyAdjustmentFactor { get; set; } = 0.1;

    [FirestoreProperty("minQuestionsPerSession")]
    public int MinQuestionsPerSession { get; set; } = 5;

    [FirestoreProperty("maxQuestionsPerSession")]
    public int MaxQuestionsPerSession { get; set; } = 20;
}
```





### Batch Operation Models
```csharp
public class BatchGetRequest
{
    [JsonPropertyName("documents")]
    public List<string> Documents { get; set; } = new();

    [JsonPropertyName("mask")]
    public DocumentMask? Mask { get; set; }

    [JsonPropertyName("transaction")]
    public string? Transaction { get; set; }

    [JsonPropertyName("newTransaction")]
    public TransactionOptions? NewTransaction { get; set; }

    [JsonPropertyName("readTime")]
    public string? ReadTime { get; set; }
}

public class BatchGetResponse
{
    [JsonPropertyName("found")]
    public List<FirestoreDocument> Found { get; set; } = new();

    [JsonPropertyName("missing")]
    public List<string> Missing { get; set; } = new();

    [JsonPropertyName("transaction")]
    public string? Transaction { get; set; }

    [JsonPropertyName("readTime")]
    public string? ReadTime { get; set; }
}

public class BatchWriteRequest
{
    [JsonPropertyName("writes")]
    public List<Write> Writes { get; set; } = new();

    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

public class BatchWriteResponse
{
    [JsonPropertyName("writeResults")]
    public List<WriteResult> WriteResults { get; set; } = new();

    [JsonPropertyName("status")]
    public List<Status> Status { get; set; } = new();
}

public class Write
{
    [JsonPropertyName("update")]
    public FirestoreDocument? Update { get; set; }

    [JsonPropertyName("delete")]
    public string? Delete { get; set; }

    [JsonPropertyName("transform")]
    public DocumentTransform? Transform { get; set; }

    [JsonPropertyName("updateMask")]
    public DocumentMask? UpdateMask { get; set; }

    [JsonPropertyName("currentDocument")]
    public Precondition? CurrentDocument { get; set; }
}

public class WriteResult
{
    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    [JsonPropertyName("transformResults")]
    public List<FirestoreValue>? TransformResults { get; set; }
}
```

### Transaction Models
```csharp
public class BeginTransactionRequest
{
    [JsonPropertyName("options")]
    public TransactionOptions? Options { get; set; }
}

public class BeginTransactionResponse
{
    [JsonPropertyName("transaction")]
    public string Transaction { get; set; } = string.Empty;
}

public class CommitRequest
{
    [JsonPropertyName("writes")]
    public List<Write> Writes { get; set; } = new();

    [JsonPropertyName("transaction")]
    public string? Transaction { get; set; }
}

public class CommitResponse
{
    [JsonPropertyName("writeResults")]
    public List<WriteResult> WriteResults { get; set; } = new();

    [JsonPropertyName("commitTime")]
    public string? CommitTime { get; set; }
}

public class TransactionOptions
{
    [JsonPropertyName("readOnly")]
    public ReadOnly? ReadOnly { get; set; }

    [JsonPropertyName("readWrite")]
    public ReadWrite? ReadWrite { get; set; }
}

public class ReadOnly
{
    [JsonPropertyName("readTime")]
    public string? ReadTime { get; set; }
}

public class ReadWrite
{
    [JsonPropertyName("retryTransaction")]
    public string? RetryTransaction { get; set; }
}
```

## Domain Models for LinguaLearn

### User Models
```csharp
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int XP { get; set; }
    public int Level { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime LastActivityDate { get; set; }
    public List<string> EarnedBadges { get; set; } = new();
    public UserPreferences Preferences { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserPreferences
{
    public string PreferredLanguage { get; set; } = "en";
    public bool SoundEnabled { get; set; } = true;
    public bool NotificationsEnabled { get; set; } = true;
    public string DifficultyLevel { get; set; } = "beginner";
    public int DailyGoalMinutes { get; set; } = 15;
}
```

### Lesson Models
```csharp
public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<string> Prerequisites { get; set; } = new();
    public List<LessonSection> Sections { get; set; } = new();
    public int EstimatedDurationMinutes { get; set; }
    public int XPReward { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LessonSection
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "vocabulary", "grammar", "pronunciation", "quiz"
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public int Order { get; set; }
}
```

### Progress Models
```csharp
public class ProgressRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Accuracy { get; set; }
    public int TimeSpentSeconds { get; set; }
    public int XPEarned { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class UserProgress
{
    public string UserId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public double OverallAccuracy { get; set; }
    public int TotalTimeSpent { get; set; }
    public int TotalXPEarned { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime FirstAttempt { get; set; }
    public DateTime LastAttempt { get; set; }
    public int AttemptCount { get; set; }
}
```

### Quiz Models
```csharp
public class Quiz
{
    public string Id { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<QuizQuestion> Questions { get; set; } = new();
    public int TimeLimit { get; set; }
    public int PassingScore { get; set; }
    public AdaptiveConfig AdaptiveSettings { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class QuizQuestion
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "multiple_choice", "fill_blank", "matching", etc.
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public int Points { get; set; } = 1;
    public string Difficulty { get; set; } = "medium";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AdaptiveConfig
{
    public bool IsEnabled { get; set; } = true;
    public double DifficultyAdjustmentFactor { get; set; } = 0.1;
    public int MinQuestionsPerSession { get; set; } = 5;
    public int MaxQuestionsPerSession { get; set; } = 20;
}
```

## Service Implementation

### IFirestoreRepository Interface
```csharp
public interface IFirestoreRepository
{
    // Document Operations
    Task<T?> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class;
    Task<DocumentReference> CreateDocumentAsync<T>(string collection, T entity, string? documentId = null, CancellationToken ct = default) where T : class;
    Task UpdateDocumentAsync<T>(string collection, string documentId, T entity, CancellationToken ct = default) where T : class;
    Task DeleteDocumentAsync(string collection, string documentId, CancellationToken ct = default);
    Task<bool> DocumentExistsAsync(string collection, string documentId, CancellationToken ct = default);

    // Query Operations
    Task<List<T>> QueryCollectionAsync<T>(string collection, Func<Query, Query>? queryBuilder = null, CancellationToken ct = default) where T : class;
    Task<List<T>> QueryCollectionGroupAsync<T>(string collectionGroup, Func<Query, Query>? queryBuilder = null, CancellationToken ct = default) where T : class;

    // Batch Operations
    Task<List<T?>> BatchGetAsync<T>(string collection, List<string> documentIds, CancellationToken ct = default) where T : class;
    Task BatchWriteAsync(Action<WriteBatch> batchBuilder, CancellationToken ct = default);

    // Transaction Operations
    Task<TResult> RunTransactionAsync<TResult>(Func<Transaction, Task<TResult>> transactionFunc, CancellationToken ct = default);

    // Realtime Operations
    IAsyncEnumerable<T?> ListenToDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class;
    IAsyncEnumerable<List<T>> ListenToCollectionAsync<T>(string collection, Func<Query, Query>? queryBuilder = null, CancellationToken ct = default) where T : class;

    // Utility Operations
    CollectionReference GetCollection(string collection);
    DocumentReference GetDocument(string collection, string documentId);
}
```



### FirestoreRepository Implementation
```csharp
public class FirestoreRepository : IFirestoreRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirestoreRepository> _logger;

    public FirestoreRepository(
        FirestoreDb firestoreDb,
        ILogger<FirestoreRepository> logger)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
    }

    public async Task<T?> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync(ct);

            if (!snapshot.Exists)
            {
                return null;
            }

            return snapshot.ConvertTo<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document {DocumentId} from collection {Collection}", documentId, collection);
            throw;
        }
    }

    public async Task<DocumentReference> CreateDocumentAsync<T>(string collection, T entity, string? documentId = null, CancellationToken ct = default) where T : class
    {
        try
        {
            var collectionRef = _firestoreDb.Collection(collection);
            
            DocumentReference docRef;
            if (!string.IsNullOrEmpty(documentId))
            {
                docRef = collectionRef.Document(documentId);
                await docRef.SetAsync(entity, cancellationToken: ct);
            }
            else
            {
                docRef = await collectionRef.AddAsync(entity, ct);
            }

            return docRef;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create document in collection {Collection}", collection);
            throw;
        }
    }

    public async Task UpdateDocumentAsync<T>(string collection, string documentId, T entity, CancellationToken ct = default) where T : class
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.SetAsync(entity, SetOptions.MergeAll, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document {DocumentId} in collection {Collection}", documentId, collection);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string collection, string documentId, CancellationToken ct = default)
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.DeleteAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId} from collection {Collection}", documentId, collection);
            throw;
        }
    }

    public async Task<bool> DocumentExistsAsync(string collection, string documentId, CancellationToken ct = default)
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync(ct);
            return snapshot.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if document {DocumentId} exists in collection {Collection}", documentId, collection);
            return false;
        }
    }

    public FirestoreQueryBuilder<T> Collection<T>(string collection) where T : class
    {
        var db = _dbProvider.GetDatabaseAsync().GetAwaiter().GetResult();
        var collectionRef = db.Collection(collection);
        return new FirestoreQueryBuilder<T>(collectionRef);
    }

    public async Task<List<T>> QueryCollectionAsync<T>(string collection, Func<FirestoreQueryBuilder<T>, FirestoreQueryBuilder<T>>? queryBuilder = null, CancellationToken ct = default) where T : class
    {
        try
        {
            var db = await _dbProvider.GetDatabaseAsync(ct);
            var collectionRef = db.Collection(collection);
            var builder = new FirestoreQueryBuilder<T>(collectionRef);
            
            if (queryBuilder != null)
            {
                builder = queryBuilder(builder);
            }

            return await builder.GetAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query collection {Collection}", collection);
            throw;
        }
    }

    public async Task<List<T>> QueryCollectionGroupAsync<T>(string collectionGroup, Func<Query, Query>? queryBuilder = null, CancellationToken ct = default) where T : class
    {
        try
        {
            var db = await _dbProvider.GetDatabaseAsync(ct);
            var query = db.CollectionGroup(collectionGroup);
            
            if (queryBuilder != null)
            {
                query = queryBuilder(query);
            }

            var snapshot = await query.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query collection group {CollectionGroup}", collectionGroup);
            throw;
        }
    }

    public async Task<List<T?>> BatchGetAsync<T>(string collection, List<string> documentIds, CancellationToken ct = default) where T : class
    {
        try
        {
            var db = await _dbProvider.GetDatabaseAsync(ct);
            var documentRefs = documentIds.Select(id => db.Collection(collection).Document(id)).ToList();
            var snapshots = await db.GetAllSnapshotsAsync(documentRefs, ct);
            
            return snapshots.Select(snapshot => snapshot.Exists ? snapshot.ConvertTo<T>() : default(T)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch get documents from collection {Collection}", collection);
            throw;
        }
    }

    public async Task BatchWriteAsync(Action<WriteBatch> batchBuilder, CancellationToken ct = default)
    {
        try
        {
            var db = await _dbProvider.GetDatabaseAsync(ct);
            var batch = db.StartBatch();
            batchBuilder(batch);
            await batch.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute batch write operations");
            throw;
        }
    }

    public async Task<TResult> RunTransactionAsync<TResult>(Func<Transaction, Task<TResult>> transactionFunc, CancellationToken ct = default)
    {
        try
        {
            var db = await _dbProvider.GetDatabaseAsync(ct);
            return await db.RunTransactionAsync(transactionFunc, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute transaction");
            throw;
        }
    }

    public async IAsyncEnumerable<T?> ListenToDocumentAsync<T>(string collection, string documentId, [EnumeratorCancellation] CancellationToken ct = default) where T : class
    {
        var db = await _dbProvider.GetDatabaseAsync(ct);
        var docRef = db.Collection(collection).Document(documentId);
        
        var listener = docRef.Listen(snapshot =>
        {
            // This will be handled by the async enumerable
        });

        try
        {
            await foreach (var snapshot in docRef.StreamAsync(ct))
            {
                yield return snapshot.Exists ? snapshot.ConvertTo<T>() : default(T);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

   





## Dependency Injection Setup

### MauiProgram.cs Configuration
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: false);
        
        // Firebase Configuration
        builder.Services.Configure<FirebaseAuthConfig>(
            builder.Configuration.GetSection("FirebaseAuth"));

        // HTTP Client for Firebase Auth (Refit)
        builder.Services.AddHttpClient();
        
        // Firebase Auth API Client (Refit)
        builder.Services.AddRefitClient<IFirebaseAuthApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<FirebaseAuthConfig>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

      

        // Repository and Services
        builder.Services.AddScoped<IFirestoreRepository, FirestoreRepository>();

        // Core Firebase Services
        builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        
        // Platform Services
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

        // Business Services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ILessonService, LessonService>();
        builder.Services.AddScoped<IProgressService, ProgressService>();
        builder.Services.AddScoped<IQuizService, QuizService>();
        builder.Services.AddScoped<IGamificationService, GamificationService>();

        // ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<LessonsViewModel>();
        builder.Services.AddTransient<LessonPlayerViewModel>();
        builder.Services.AddTransient<QuizViewModel>();
        builder.Services.AddTransient<ProgressViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        return builder.Build();
    }
}
```

### Configuration Files

#### appsettings.json
```json
{
  "FirebaseAuth": {
    "ApiKey": "your-firebase-api-key",
    "ProjectId": "your-project-id"
  }
}
```

#### Firebase Service Account Setup
1. Go to Firebase Console → Project Settings → Service Accounts
2. Click "Generate new private key" to download the JSON file
3. Rename the file to `firebase-adminsdk.json`
4. Add the file to your MAUI project under `Resources/Raw/` folder
5. Set the file's Build Action to "MauiAsset"

The `firebase-adminsdk.json` file structure:
```json
{
  "type": "service_account",
  "project_id": "your-project-id",
  "private_key_id": "key-id",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@your-project-id.iam.gserviceaccount.com",
  "client_id": "client-id",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40your-project-id.iam.gserviceaccount.com"
}
```

## Business Service Examples

### User Service
```csharp
public interface IUserService
{
    Task<User?> GetUserAsync(string userId, CancellationToken ct = default);
    Task<User> CreateUserAsync(User user, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    Task DeleteUserAsync(string userId, CancellationToken ct = default);
    Task<User?> GetCurrentUserAsync(CancellationToken ct = default);
    Task UpdateUserXPAsync(string userId, int xpToAdd, CancellationToken ct = default);
    Task UpdateUserStreakAsync(string userId, CancellationToken ct = default);
    Task<List<User>> GetLeaderboardAsync(int limit = 10, CancellationToken ct = default);
}

public class UserService : IUserService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<UserService> _logger;
    private const string USERS_COLLECTION = "users";

    public UserService(
        IFirestoreRepository firestoreRepository,
        IFirebaseAuthService authService,
        ILogger<UserService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken ct = default)
    {
        return await _firestoreRepository.GetDocumentAsync<User>(USERS_COLLECTION, userId, ct);
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken ct = default)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        var docRef = await _firestoreRepository.CreateDocumentAsync(USERS_COLLECTION, user, user.Id, ct);
        user.Id = docRef.Id;
        
        return user;
    }

    public async Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _firestoreRepository.UpdateDocumentAsync(USERS_COLLECTION, user.Id, user, ct);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        await _firestoreRepository.DeleteDocumentAsync(USERS_COLLECTION, userId, ct);
    }

    public async Task<User?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var session = await _authService.GetCurrentSessionAsync();
        if (session == null) return null;

        return await GetUserAsync(session.UserId, ct);
    }

    public async Task UpdateUserXPAsync(string userId, int xpToAdd, CancellationToken ct = default)
    {
        await _firestoreRepository.RunTransactionAsync(async transaction =>
        {
            var docRef = _firestoreRepository.GetDocument(USERS_COLLECTION, userId);
            var snapshot = await transaction.GetSnapshotAsync(docRef);
            
            if (snapshot.Exists)
            {
                var user = snapshot.ConvertTo<User>();
                user.XP += xpToAdd;
                user.Level = CalculateLevel(user.XP);
                user.UpdatedAt = DateTime.UtcNow; // Using DateTime with converter
                
                transaction.Set(docRef, user, SetOptions.MergeAll);
            }
            
            return true;
        }, ct);
    }

    public async Task UpdateUserStreakAsync(string userId, CancellationToken ct = default)
    {
        await _firestoreRepository.RunTransactionAsync(async transaction =>
        {
            var docRef = _firestoreRepository.GetDocument(USERS_COLLECTION, userId);
            var snapshot = await transaction.GetSnapshotAsync(docRef);
            
            if (snapshot.Exists)
            {
                var user = snapshot.ConvertTo<User>();
                var today = DateTime.UtcNow.Date;
                var lastActivity = user.LastActivityDate.Date;
                
                if (lastActivity == today.AddDays(-1))
                {
                    // Continue streak
                    user.CurrentStreak++;
                }
                else if (lastActivity < today.AddDays(-1))
                {
                    // Streak broken
                    user.CurrentStreak = 1;
                }
                // If lastActivity == today, streak stays the same
                
                user.LongestStreak = Math.Max(user.LongestStreak, user.CurrentStreak);
                user.LastActivityDate = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                
                transaction.Set(docRef, user, SetOptions.MergeAll);
            }
            
            return true;
        }, ct);
    }

    public async Task<List<User>> GetLeaderboardAsync(int limit = 10, CancellationToken ct = default)
    {
        return await _firestoreRepository.QueryCollectionAsync<User>(USERS_COLLECTION, 
            query => query.OrderByDescending("xp").Limit(limit), ct);
    }

    private int CalculateLevel(int xp)
    {
        // Level XP = 50 * level^1.7
        return (int)Math.Floor(Math.Pow(xp / 50.0, 1.0 / 1.7));
    }
}
```

## Error Handling and Resilience

### Firestore Error Handling
```csharp
public class FirestoreException : Exception
{
    public FirestoreErrorCode ErrorCode { get; }
    public string? Details { get; }

    public FirestoreException(FirestoreErrorCode errorCode, string message, string? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}

public enum FirestoreErrorCode
{
    Unknown,
    PermissionDenied,
    NotFound,
    AlreadyExists,
    InvalidArgument,
    DeadlineExceeded,
    Unavailable,
    Unauthenticated,
    ResourceExhausted,
    FailedPrecondition,
    Aborted,
    OutOfRange,
    Unimplemented,
    Internal,
    DataLoss
}
```

### Retry Policy with Polly
```csharp
public class FirestoreServiceWithRetry : IFirestoreService
{
    private readonly IFirestoreService _innerService;
    private readonly IAsyncPolicy _retryPolicy;

    public FirestoreServiceWithRetry(IFirestoreService innerService)
    {
        _innerService = innerService;
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<FirestoreException>(ex => ex.ErrorCode == FirestoreErrorCode.Unavailable)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                });
    }

    public async Task<T?> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _innerService.GetDocumentAsync<T>(collection, documentId, ct));
    }

    // Implement other methods with retry policy...
}
```

## Dependency Injection Best Practices



**Pattern 3: Extension Method (Clean)**
```csharp
// Clean extension method approach
builder.Services.AddFirestore(builder.Configuration);
```

### Service Lifetimes

- **FirestoreDb**: Singleton (expensive to create, thread-safe)
- **IFirestoreDbFactory**: Singleton (stateless factory)
- **IFirestoreService**: Singleton (manages authentication state)
- **IFirestoreRepository**: Scoped (per request/operation)
- **Business Services**: Scoped (per request/operation)

### Why This Pattern Works

1. **Separation of Concerns**: Factory handles creation, Service handles auth, Repository handles operations
2. **Testability**: Easy to mock interfaces for unit testing
3. **Performance**: Singleton FirestoreDb instances are reused
4. **Flexibility**: Can switch between authenticated and unauthenticated databases
5. **Thread Safety**: FirestoreDb is thread-safe and can be shared

## Authentication Integration

### Firebase ID Token to Google Credential
The `FirestoreDbFactory` handles the conversion from Firebase ID tokens to Google credentials:

```csharp
// In FirestoreDbFactory.CreateAuthenticatedDatabase
var credential = GoogleCredential.FromAccessToken(idToken);
```

### Automatic Token Refresh
The service checks token expiration and automatically refreshes when needed:

```csharp
// Check if we can reuse cached database
if (_cachedDb != null && _lastToken == session.IdToken)
{
    return _cachedDb;
}
```

### Application Default Credentials (ADC)
For production deployments, use ADC instead of service account keys:

```csharp
// Automatically uses ADC (recommended for production)
builder.Credential = GoogleCredential.GetApplicationDefault();
```

ADC automatically finds credentials in this order:
1. `GOOGLE_APPLICATION_CREDENTIALS` environment variable
2. User credentials from `gcloud auth application-default login`
3. Service account attached to the resource (GCE, Cloud Run, etc.)
4. Google Cloud SDK default credentials

### Security Rules Integration
Firestore security rules work seamlessly with Firebase Authentication:

```javascript
// Example Firestore security rules
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Users can read/write their own data
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Lessons are public read, admin write
    match /lessons/{lessonId} {
      allow read: if true;
      allow write: if request.auth != null && 
        request.auth.token.admin == true;
    }
    
    // Progress records are user-specific
    match /progress/{userId}/records/{recordId} {
      allow read, write: if request.auth != null && 
        request.auth.uid == userId;
    }
  }
}
```





### Connection Management
- Reuse FirestoreDb instances (singleton pattern)
- Use connection pooling automatically handled by the SDK
- Monitor connection health with listeners

### Memory Management
- Dispose of listeners when no longer needed
- Use weak references for long-lived listeners
- Clear caches periodically in long-running apps

## Security Considerations

### Authentication Integration
- Always validate authentication before Firestore operations
- Implement automatic token refresh
- Handle authentication failures gracefully

### Data Validation
- Validate all input data before sending to Firestore
- Implement server-side validation rules
- Sanitize user inputs

### Security Rules Integration
- Design Firestore security rules to match client operations
- Test security rules thoroughly
- Implement proper user authorization checks

This comprehensive Firestore specification provides a robust foundation for data operations in the LinguaLearn MAUI application, with proper integration with Firebase Authentication, offline support, and following MAUI dependency injection best practices.