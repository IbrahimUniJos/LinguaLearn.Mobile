using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LinguaLearn.Mobile.Extensions;
using Refit;
using System.Reflection;


namespace LinguaLearn.Mobile
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Add configuration
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("LinguaLearn.Mobile.Resources.Raw.appsettings.json");
            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
                builder.Configuration.AddConfiguration(config);
            }

            // Add services
            builder.Services.AddSecureStorage();
            builder.Services.AddFirebaseServices(builder.Configuration);

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
