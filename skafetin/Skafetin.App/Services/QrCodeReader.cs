namespace Skafetin.App.Services;

public static class QrCodeReader
{
    private const string EquipmentSegment = "equipment";

    public static bool TryGetEquipmentId(string? rawValue, out int equipmentId)
    {
        equipmentId = 0;

        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        var text = rawValue.Trim();

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            if (!Uri.TryCreate(new Uri("http://placeholder/"), text, out uri))
                return false;
        }

        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
            return false;

        if (!string.Equals(segments[^2], EquipmentSegment, StringComparison.OrdinalIgnoreCase))
            return false;

        return int.TryParse(segments[^1], out equipmentId) && equipmentId > 0;
    }
}
