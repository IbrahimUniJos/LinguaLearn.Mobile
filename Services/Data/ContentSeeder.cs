using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Data;

/// <summary>
/// Facade to run content seeding safely from app startup or dev menu.
/// </summary>
public class ContentSeeder
{
    private readonly ContentSeedService _seedService;
    private readonly ILogger<ContentSeeder> _logger;

    public ContentSeeder(ContentSeedService seedService, ILogger<ContentSeeder> logger)
    {
        _seedService = seedService;
        _logger = logger;
    }

    public async Task<bool> TrySeedAsync(string assetFile = "seed-lessons-quizzes.json", CancellationToken ct = default)
    {
        try
        {
            await _seedService.SeedFromAssetAsync(assetFile, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content seeding failed from asset {Asset}", assetFile);
            return false;
        }
    }
}
