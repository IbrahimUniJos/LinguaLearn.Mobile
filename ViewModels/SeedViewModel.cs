using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.ViewModels;

public partial class SeedViewModel : ObservableObject
{
    private readonly ContentSeedService _seedService;
    private readonly ILogger<SeedViewModel> _logger;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string status = "Idle";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isCompleted;

    public SeedViewModel(ContentSeedService seedService, ILogger<SeedViewModel> logger)
    {
        _seedService = seedService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task SeedAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            IsCompleted = false;
            ProgressValue = 0;
            Status = "Preparing...";

            var reporter = new System.Progress<SeedProgress>(p =>
            {
                ProgressValue = p.Percent;
                Status = $"{p.Phase}: {p.Message}";
            });

            await _seedService.SeedFromAssetAsync("seed-lessons-quizzes.json", reporter, CancellationToken.None);

            IsCompleted = true;
            Status = "Completed";
            ProgressValue = 1.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seeding failed");
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
