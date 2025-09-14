using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;
using LinguaLearn.Mobile.Configuration;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.Services.Data;
using LinguaLearn.Mobile.Services.Storage;

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

        // Register Firebase Auth API with Refit
        var firebaseAuthConfig = configuration.GetSection("Firebase:Auth").Get<FirebaseAuthConfig>() ?? new FirebaseAuthConfig();
        
        services.AddRefitClient<IFirebaseAuthApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(firebaseAuthConfig.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(firebaseAuthConfig.HttpTimeoutSeconds);
            });

        // Register Firebase Auth Service
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        // Register Firestore
        services.AddSingleton<FirestoreDb>(serviceProvider =>
        {
            var firestoreConfig = configuration.GetSection("Firebase:Firestore").Get<FirestoreConfig>() ?? new FirestoreConfig();
            var logger = serviceProvider.GetRequiredService<ILogger<FirestoreDb>>();
            
            try
            {
                if (firestoreConfig.UseEmulator)
                {
                    Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", firestoreConfig.EmulatorHost);
                    return FirestoreDb.Create(firestoreConfig.ProjectId);
                }
                
                // Load credentials file from app package
                string jsonCredentials;
                using var stream = FileSystem.OpenAppPackageFileAsync(firestoreConfig.CredentialsFileName).GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                jsonCredentials = reader.ReadToEnd();

                var builder = new FirestoreDbBuilder
                {
                    ProjectId = firestoreConfig.ProjectId,
                    JsonCredentials = jsonCredentials
                };

                return builder.Build();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Firestore: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to initialize Firestore: {ex.Message}", ex);
            }
        });

        // Register Firestore Repository
        services.AddScoped<IFirestoreRepository, FirestoreRepository>();

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