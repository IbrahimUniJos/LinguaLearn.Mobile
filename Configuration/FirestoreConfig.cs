namespace LinguaLearn.Mobile.Configuration;

public class FirestoreConfig
{
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsFileName { get; set; } = "google-services.json";
    public bool UseEmulator { get; set; } = false;
    public string EmulatorHost { get; set; } = "localhost:8080";
}