namespace Skafetin.Api.Storage;

/// <summary>
/// Jedno mjesto na kojem se računaju putanje do datoteka opreme. Koriste ga
/// EquipmentMediaController (upload i preuzimanje) i SeedData (demo datoteke),
/// da se pravilo za putanju ne piše na dva mjesta.
/// </summary>
public static class MediaStorage
{
    private const string DefaultUploadPath = "uploads/equipment";
    private const string SeedFilesPath = "SeedFiles/equipment";

    /// <summary>Mapa u koju se spremaju uploadane datoteke. Nije u gitu.</summary>
    public static string GetUploadDirectory(IConfiguration configuration, string contentRootPath)
    {
        var configured = configuration["Storage:EquipmentMediaPath"];

        if (string.IsNullOrWhiteSpace(configured))
            configured = DefaultUploadPath;

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(contentRootPath, configured);
    }

    /// <summary>Mapa s demo datotekama koje se commitaju u git i koje seed kopira.</summary>
    public static string GetSeedFilesDirectory(string contentRootPath) =>
        Path.Combine(contentRootPath, SeedFilesPath);
}
