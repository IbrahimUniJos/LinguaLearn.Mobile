using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LinguaLearn.Mobile.Extensions;
using Refit;
using System.Reflection;
using System.IO;
using HorusStudio.Maui.MaterialDesignControls;
using LinguaLearn.Mobile.ViewModels;
using LinguaLearn.Mobile.Views.Auth;
using LinguaLearn.Mobile.Views.Onboarding;
using LinguaLearn.Mobile.Views;
using LinguaLearn.Mobile.Services.Activity;
using LinguaLearn.Mobile.Services.Data;
using CommunityToolkit.Maui;
using LinguaLearn.Mobile.ViewModels.Auth;


namespace LinguaLearn.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMaterialDesignControls()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Add configuration
            using (var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
            using (var reader = new StreamReader(stream))
            {
                var jsonContent = reader.ReadToEnd();
                
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)))
                        .Build();
                    builder.Configuration.AddConfiguration(config);
                }
            }

            // Add services
            builder.Services.AddSecureStorage();
            builder.Services.AddFirebaseServices(builder.Configuration);
            builder.Services.AddTransient<IActivityService, ActivityService>();
            builder.Services.AddTransient<TestDataSeeder>();
            
            // Add game logic services
            builder.Services.AddTransient<Services.Lessons.ILessonService, Services.Lessons.LessonService>();
            builder.Services.AddTransient<Services.Quizzes.IQuizService, Services.Quizzes.QuizService>();

            // Add ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<UserHomepageViewModel>();
            builder.Services.AddTransient<LessonsViewModel>();
            builder.Services.AddTransient<LessonPlayerViewModel>();
            builder.Services.AddTransient<QuizViewModel>();

            // Add Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<Views.UserHomepagePage>();
            builder.Services.AddTransient<Views.LessonsPage>();
            builder.Services.AddTransient<Views.LessonPlayerPage>();
            builder.Services.AddTransient<Views.LessonCompletePage>();
            builder.Services.AddTransient<Views.QuizPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
