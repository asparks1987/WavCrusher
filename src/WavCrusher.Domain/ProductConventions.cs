namespace WavCrusher.Domain;

/// <summary>
/// Stable product identifiers used by manifests, reports, and path validation.
/// </summary>
public static class ProductConventions
{
    public const string ProductName = "WavCrusher";
    public const string ManifestFormat = "wavcrusher-archive-manifest";
    public const string JournalFormat = "wavcrusher-operation-journal";
    public const string ReportFormat = "wavcrusher-operation-report";
    public const string SettingsFormat = "wavcrusher-settings";
    public const string ReservedMetadataDirectory = ".wavcrusher";
}
