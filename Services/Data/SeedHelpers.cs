using System.Text.Json;

namespace LinguaLearn.Mobile.Services.Data;

public static class SeedHelpers
{
    public static async Task<string> ReadEmbeddedJsonAsync(string fileName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
