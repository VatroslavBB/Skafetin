using MudBlazor;

namespace Skafetin.App.Theme;

public static class SkafetinTheme
{
    public const string Plava      = "#123A63";
    public const string PlavaTamna = "#0E2C4E";
    public const string PlavaSvijetla = "#4E8DCB";
    public const string Zlatna     = "#C08D2F";
    public const string ZlatnaSvijetla = "#E3B457";
    public const string Kamen      = "#F3F5F7";
    public const string Tinta      = "#131C26";

    public static readonly MudTheme Instance = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Plava,
            PrimaryContrastText = "#FFFFFF",
            Secondary = Zlatna,
            SecondaryContrastText = "#FFFFFF",
            Tertiary = PlavaSvijetla,
            AppbarBackground = PlavaTamna,
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#2C3947",
            DrawerIcon = "#5A6875",
            Background = "#E9ECEF",
            Surface = "#FFFFFF",
            TextPrimary = Tinta,
            TextSecondary = "#4E5C6B",
            ActionDefault = "#5A6875",
            Divider = "#D7DDE4",
            LinesDefault = "#D7DDE4",
            TableLines = "#E2E7EC",
            Success = "#2E7D5B",
            Warning = "#B77A17",
            Error = "#B3382F",
            Info = Plava
        },
        PaletteDark = new PaletteDark
        {
            Primary = PlavaSvijetla,
            PrimaryContrastText = "#08131F",
            Secondary = ZlatnaSvijetla,
            SecondaryContrastText = "#1A1204",
            Tertiary = "#8FB8DF",
            AppbarBackground = "#0E1922",
            AppbarText = "#E7EDF3",
            DrawerBackground = "#101922",
            DrawerText = "#C3CEDA",
            DrawerIcon = "#8FA0B1",
            Background = "#0D131A",
            Surface = "#161E27",
            TextPrimary = "#E7EDF3",
            TextSecondary = "#98A6B4",
            ActionDefault = "#98A6B4",
            Divider = "#26313C",
            LinesDefault = "#26313C",
            TableLines = "#26313C",
            Success = "#4CAF86",
            Warning = ZlatnaSvijetla,
            Error = "#E1685E",
            Info = PlavaSvijetla
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "260px"
        }
    };
}
