using MudBlazor;

namespace Skafetin.App.Theme;

public static class SkafetinTheme
{
    // Županijske boje - ne mijenjaju se.
    public const string Plava      = "#123A63";
    public const string PlavaTamna = "#0E2C4E";
    public const string PlavaSvijetla = "#4E8DCB";
    public const string Zlatna     = "#C08D2F";
    public const string ZlatnaSvijetla = "#E3B457";
    public const string Kamen      = "#F3F5F7";
    public const string Tinta      = "#131C26";

    private static readonly string[] Pismo =
        ["IBM Plex Sans", "Segoe UI", "system-ui", "Helvetica", "Arial", "sans-serif"];

    public static readonly MudTheme Instance = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Plava,
            PrimaryContrastText = "#FFFFFF",
            Secondary = Zlatna,
            SecondaryContrastText = "#241A03",
            Tertiary = PlavaSvijetla,

            // Traka je svijetla, stup je taman - obrnuto od prve verzije.
            AppbarBackground = "#FFFFFF",
            AppbarText = Tinta,
            DrawerBackground = PlavaTamna,
            DrawerText = "#B9C9DA",
            DrawerIcon = "#7E97B1",

            Background = Kamen,
            Surface = "#FFFFFF",
            TextPrimary = Tinta,
            TextSecondary = "#5A6A7A",
            ActionDefault = "#5A6875",
            Divider = "#DFE5EB",
            LinesDefault = "#E1E7ED",
            TableLines = "#E7ECF1",
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

            AppbarBackground = "#161E27",
            AppbarText = "#E7EDF3",
            DrawerBackground = "#0A1119",
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

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = Pismo,
                FontSize = ".875rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0"
            },

            // Naslov stranice. Krupnije i teže nego MudBlazor default (2.125rem / 400).
            H4 = new H4Typography
            {
                FontFamily = Pismo,
                FontSize = "1.75rem",
                FontWeight = "600",
                LineHeight = "1.2",
                LetterSpacing = "-.02em"
            },
            H5 = new H5Typography
            {
                FontFamily = Pismo,
                FontSize = "1.375rem",
                FontWeight = "600",
                LineHeight = "1.3",
                LetterSpacing = "-.015em"
            },
            H6 = new H6Typography
            {
                FontFamily = Pismo,
                FontSize = "1.0625rem",
                FontWeight = "600",
                LineHeight = "1.4",
                LetterSpacing = "-.01em"
            },

            Subtitle1 = new Subtitle1Typography
            {
                FontFamily = Pismo,
                FontSize = ".9375rem",
                FontWeight = "600",
                LineHeight = "1.5",
                LetterSpacing = "0"
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontFamily = Pismo,
                FontSize = ".8125rem",
                FontWeight = "600",
                LineHeight = "1.45",
                LetterSpacing = ".01em"
            },

            Body1 = new Body1Typography
            {
                FontFamily = Pismo,
                FontSize = ".9375rem",
                FontWeight = "400",
                LineHeight = "1.55",
                LetterSpacing = "0"
            },
            Body2 = new Body2Typography
            {
                FontFamily = Pismo,
                FontSize = ".8125rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = ".005em"
            },

            // Bez verzala na gumbima - najveća razlika u odnosu na Material default.
            Button = new ButtonTypography
            {
                FontFamily = Pismo,
                FontSize = ".8125rem",
                FontWeight = "600",
                LineHeight = "1.6",
                LetterSpacing = ".01em",
                TextTransform = "none"
            },

            // Caption ostaje normalnog sloga - nosi nazive datoteka i slobodan tekst.
            Caption = new CaptionTypography
            {
                FontFamily = Pismo,
                FontSize = ".75rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = ".01em"
            },

            // Overline je oznaka: sitna, verzalna, prorijeđena. Koristi se za
            // naslove cjelina u izborniku i (od faze D2) za oznake kartica.
            Overline = new OverlineTypography
            {
                FontFamily = Pismo,
                FontSize = ".6875rem",
                FontWeight = "600",
                LineHeight = "1.6",
                LetterSpacing = ".1em",
                TextTransform = "uppercase"
            }
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
            DrawerWidthLeft = "248px"
        }
    };
}
