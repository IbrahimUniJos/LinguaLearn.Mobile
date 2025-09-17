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

            // Add ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();

            // Add Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<OnboardingPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
