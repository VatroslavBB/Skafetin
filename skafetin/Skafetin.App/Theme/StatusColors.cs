using MudBlazor;

namespace Skafetin.App.Theme;

/// <summary>
/// Jedno mjesto na kojem status postaje boja. Prije je istih devet privatnih
/// metoda stajalo po stranicama, pa je promjena boje značila obilazak svih.
/// Nazivi statusa dolaze iz šifrarnika, a identifikatori iz seeda.
/// </summary>
public static class StatusColors
{
    // Zahtjev za opremom - EquipmentRequestStatus
    public const int RequestReceived   = 1;
    public const int RequestInProgress = 2;
    public const int RequestApproved   = 3;
    public const int RequestRejected   = 4;
    public const int RequestFulfilled  = 5;
    public const int RequestClosed     = 6;

    // Zahtjev za otpisom - WriteOffRequestStatus
    public const int WriteOffReceived   = 1;
    public const int WriteOffInProgress = 2;
    public const int WriteOffApproved   = 3;
    public const int WriteOffRejected   = 4;
    public const int WriteOffExecuted   = 5;

    public static Color Assignment(string statusName) => statusName switch
    {
        "Aktivno"     => Color.Success,
        "Vraćeno"     => Color.Default,
        "Premješteno" => Color.Info,
        "Stornirano"  => Color.Error,
        _             => Color.Default
    };

    public static Color Equipment(string statusName) => statusName switch
    {
        "Na skladištu" => Color.Success,
        "Zaduženo"     => Color.Info,
        "Na servisu"   => Color.Warning,
        "Nedostaje"    => Color.Error,
        _              => Color.Default
    };

    public static Color Inventory(string statusName) => statusName switch
    {
        "Skica"      => Color.Default,
        "Otvorena"   => Color.Info,
        "U tijeku"   => Color.Warning,
        "Završena"   => Color.Success,
        "Zaključana" => Color.Dark,
        _            => Color.Default
    };

    public static Color Request(int statusId) => statusId switch
    {
        RequestReceived   => Color.Default,
        RequestInProgress => Color.Info,
        RequestApproved   => Color.Success,
        RequestRejected   => Color.Error,
        RequestFulfilled  => Color.Primary,
        RequestClosed     => Color.Dark,
        _                 => Color.Default
    };

    public static Color WriteOff(int statusId) => statusId switch
    {
        WriteOffReceived   => Color.Default,
        WriteOffInProgress => Color.Info,
        WriteOffApproved   => Color.Success,
        WriteOffRejected   => Color.Error,
        WriteOffExecuted   => Color.Dark,
        _                  => Color.Default
    };

    /// <summary>Vrsta promjene na početnoj stranici - DashboardController.RecentActivity.</summary>
    public static Color ActivityKind(string kind) => kind switch
    {
        "Zaduženje" => Color.Info,
        "Otpis"     => Color.Error,
        "Zahtjev"   => Color.Tertiary,
        _           => Color.Default
    };
}
