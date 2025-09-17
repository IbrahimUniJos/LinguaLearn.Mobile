using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinguaLearn.Mobile.Configuration;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.Services.Data;
using LinguaLearn.Mobile.Services.Storage;
using LinguaLearn.Mobile.Services.User;
using LinguaLearn.Mobile.Services.Lessons;
using LinguaLearn.Mobile.Services.Quizzes;
using LinguaLearn.Mobile.Services.Progress;
using LinguaLearn.Mobile.Services.Audio;
using LinguaLearn.Mobile.Services.Activity;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Converters;

namespace LinguaLearn.Mobile.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFirebaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        services.Configure<FirebaseAuthConfig>(configuration.GetSection("Firebase:Auth"));
        services.Configure<FirestoreConfig>(configuration.GetSection("Firebase:Firestore"));

        // Register secure storage service
        services.AddSingleton<ISecureCredentialService, SecureCredentialService>();

        // Configure JSON serialization options for Firebase API
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // Get Firebase configuration
        var firebaseAuthConfig = configuration.GetSection("Firebase:Auth").Get<FirebaseAuthConfig>() ?? new FirebaseAuthConfig();
        
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonSerializerOptions)
        };

        // Register Firebase Auth API client for main auth operations
        // Base URL: https://identitytoolkit.googleapis.com/v1
        // This allows endpoints like /accounts:signInWithPassword?key={apiKey}
        services.AddRefitClient<IFirebaseAuthApi>(refitSettings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://identitytoolkit.googleapis.com/v1");
                client.Timeout = TimeSpan.FromSeconds(firebaseAuthConfig.HttpTimeoutSeconds);
                
                // Add standard headers for Firebase API
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "LinguaLearn-MAUI/1.0");
            });

        // Register Firebase Token API client for token refresh operations
        // Base URL: https://securetoken.googleapis.com/v1
        // This allows endpoint /token?key={apiKey}
        services.AddRefitClient<IFirebaseTokenApi>(refitSettings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://securetoken.googleapis.com/v1");
                client.Timeout = TimeSpan.FromSeconds(firebaseAuthConfig.HttpTimeoutSeconds);
                
                // Add standard headers for Firebase Token API
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "LinguaLearn-MAUI/1.0");
            });

        // Register Firebase Auth Service
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        // Register FirestoreDb singleton using bundled service account credentials
        services.AddSingleton<FirestoreDb>(serviceProvider =>
        {
            var firestoreConfig = configuration.GetSection("Firebase:Firestore").Get<FirestoreConfig>() ?? new FirestoreConfig();
            var logger = serviceProvider.GetRequiredService<ILogger<FirestoreDb>>();
            
            try
            {
                // Load credentials from app package using GoogleCredential
                using var stream = FileSystem.OpenAppPackageFileAsync(firestoreConfig.CredentialsFileName).GetAwaiter().GetResult();
                var credential = GoogleCredential.FromStream(stream);
                
                // Create converter registry with custom converters
                var converterRegistry = new ConverterRegistry
                {
                    new DifficultyLevelConverter(),
                    new PronunciationSensitivityConverter(),
                    new AppThemeConverter(),
                    new TimeSpanConverter()
                };
                
                return new FirestoreDbBuilder
                {
                    ProjectId = firestoreConfig.ProjectId,
                    Credential = credential,
                    ConverterRegistry = converterRegistry
                }.Build();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Firestore: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to initialize Firestore: {ex.Message}", ex);
            }
        });

        // Register Firestore Repository
        services.AddScoped<IFirestoreRepository, FirestoreRepository>();

        // Register Business Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<IActivityService, ActivityService>();

        return services;
    }

    public static IServiceCollection AddSecureStorage(this IServiceCollection services)
    {
        services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        return services;
    }

    public static async Task InitializeFirebaseAsync(this IServiceProvider serviceProvider, string apiKey)
    {
        var credentialService = serviceProvider.GetRequiredService<ISecureCredentialService>();
        
        // Store the API key securely
        await credentialService.SetFirebaseApiKeyAsync(apiKey);
    }
}