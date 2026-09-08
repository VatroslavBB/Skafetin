using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Ai;

public class MockAiService : IAiService
{
    public string ProviderName => AiOptions.MockProvider;
    public string ModelName => "Lokalni generator teksta";
    public bool UsesExternalService => false;

    public Task<AiSuggestionDto> SummarizeInventoryAsync(
        InventorySummaryContext context,
        CancellationToken cancellationToken = default)
    {
        var summary = context.Summary;
        var text = new StringBuilder();

        if (summary.Total == 0)
        {
            text.Append($"Inventura {context.Code} na lokaciji {context.LocationName} nema nijednu stavku, pa se sažetak ne može sastaviti.");
            return Task.FromResult(Suggestion(text.ToString()));
        }

        if (summary.Counted == 0)
        {
            text.Append($"Inventura {context.Code} na lokaciji {context.LocationName} obuhvaća {Items(summary.Total)}, ali popisivanje još nije započelo.");
            return Task.FromResult(Suggestion(text.ToString()));
        }

        text.Append($"Inventura {context.Code} provedena je na lokaciji {context.LocationName} i obuhvaća {Items(summary.Total)}. ");

        var remaining = summary.Total - summary.Counted;

        text.Append(remaining == 0
            ? "Sve stavke su popisane. "
            : $"Popisano je {summary.Counted} od {summary.Total} stavaka, a {Items(remaining)} još čeka provjeru. ");

        var findings = new List<string>();

        if (summary.Missing > 0)
            findings.Add($"{Items(summary.Missing)} nije pronađeno");

        if (summary.WrongLocation > 0)
            findings.Add($"{Items(summary.WrongLocation)} zatečeno je izvan očekivane lokacije");

        if (summary.Damaged > 0)
            findings.Add($"{Items(summary.Damaged)} evidentirano je kao oštećeno");

        if (findings.Count == 0)
        {
            text.Append("Odstupanja nisu utvrđena. ");
        }
        else
        {
            text.Append("Utvrđena su sljedeća odstupanja: ");
            text.Append(JoinCroatian(findings));
            text.Append(". ");

            if (summary.Missing > 0)
                text.Append("Za nepronađenu opremu potrebno je pokrenuti postupak otpisa ili utvrditi odgovornost. ");

            if (summary.WrongLocation > 0)
                text.Append("Opremu zatečenu na drugoj lokaciji treba uskladiti prijenosom ili ispravkom lokacije. ");
        }

        text.Append(context.IsLocked
            ? "Inventura je zaključana i brojke su konačne."
            : "Inventura još nije zaključana, pa se brojke mogu promijeniti.");

        return Task.FromResult(Suggestion(text.ToString()));
    }

    private static readonly string[] KnownManufacturers =
    [
        "Dell", "HP", "Lenovo", "Asus", "Acer", "Apple", "Samsung", "LG", "Canon",
        "Epson", "Brother", "Xerox", "Logitech", "Bosch", "Makita", "Philips", "Zebra"
    ];

    public Task<EquipmentIntakeSuggestionDto> SuggestEquipmentIntakeAsync(
        EquipmentIntakeContext context,
        CancellationToken cancellationToken = default)
    {
        var text = context.Text.Trim();
        var lower = text.ToLowerInvariant();

        var suggestion = new EquipmentIntakeSuggestionDto
        {
            Description = text,
            Manufacturer = KnownManufacturers.FirstOrDefault(
                m => lower.Contains(m.ToLowerInvariant())),
            SerialNumber = MatchGroup(text, @"(?:s/?n|serijski(?:\s+broj)?)\s*[:\-]?\s*([A-Za-z0-9\-]{4,})"),
            InventoryNumber = MatchGroup(text, @"(?:inv(?:enturni)?(?:\s+broj)?)\s*[:\-]?\s*([A-Za-z0-9\-/]{2,})") ?? string.Empty,
            EquipmentCategoryId = MatchLookup(lower, context.Categories),
            LocationId = MatchLookup(lower, context.Locations)
        };

        suggestion.Model = MatchModel(text, suggestion.Manufacturer);
        suggestion.PurchaseValue = MatchValue(text);
        suggestion.Name = BuildName(suggestion, text);

        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(suggestion.InventoryNumber))
            warnings.Add("Inventurni broj nije prepoznat - upiši ga ručno.");

        if (suggestion.EquipmentCategoryId == 0)
            warnings.Add("Kategorija nije prepoznata - odaberi je iz popisa.");

        if (suggestion.LocationId == 0)
            warnings.Add("Lokacija nije prepoznata - odaberi je iz popisa.");

        if (suggestion.PurchaseValue is null)
            warnings.Add("Nabavna vrijednost nije prepoznata.");

        if (string.IsNullOrWhiteSpace(suggestion.SerialNumber))
            warnings.Add("Serijski broj nije prepoznat.");

        warnings.Add("Prijedlog je nastao raščlanjivanjem bilješke; provjeri svako polje prije spremanja.");

        suggestion.Warnings = warnings;
        suggestion.Confidence = Confidence(suggestion);

        return Task.FromResult(suggestion);
    }

    public Task<EquipmentDataCheckDto> CheckEquipmentDataAsync(
        EquipmentCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var equipment = context.Equipment;
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(equipment.Name))
            warnings.Add("Naziv nije unesen.");

        if (string.IsNullOrWhiteSpace(equipment.InventoryNumber))
            warnings.Add("Inventurni broj nije unesen.");
        else if (context.InventoryNumberTaken)
            warnings.Add($"Inventurni broj {equipment.InventoryNumber} već postoji u evidenciji.");

        if (equipment.EquipmentCategoryId <= 0 || string.IsNullOrWhiteSpace(context.CategoryName))
            warnings.Add("Kategorija nije odabrana.");

        if (equipment.LocationId <= 0 || string.IsNullOrWhiteSpace(context.LocationName))
            warnings.Add("Lokacija nije odabrana.");

        if (equipment.EquipmentStatusId <= 0)
            warnings.Add("Status nije odabran.");

        if (!string.IsNullOrWhiteSpace(equipment.SerialNumber) && context.SerialNumberTaken)
            warnings.Add($"Serijski broj {equipment.SerialNumber} već je zabilježen na drugoj opremi.");

        if (equipment.PurchaseValue is null)
            warnings.Add("Nabavna vrijednost nije unesena, pa oprema neće ulaziti u vrijednosne izvještaje.");
        else if (equipment.PurchaseValue == 0)
            warnings.Add("Nabavna vrijednost je nula - provjeri je li to točno.");

        if (string.IsNullOrWhiteSpace(equipment.Manufacturer) && string.IsNullOrWhiteSpace(equipment.Model))
            warnings.Add("Nedostaju proizvođač i model, pa će opremu biti teže prepoznati na terenu.");

        var result = new EquipmentDataCheckDto
        {
            IsReady = warnings.Count == 0,
            Warnings = warnings,
            Summary = warnings.Count == 0
                ? "Podaci su potpuni i spremni za spremanje."
                : $"Provjera je pronašla {(warnings.Count == 1 ? "jednu stavku" : $"{warnings.Count} stavaka")} za doradu prije spremanja."
        };

        return Task.FromResult(result);
    }

    public Task<AiSuggestionDto> DraftRequestAsync(
        RequestDraftContext context,
        CancellationToken cancellationToken = default)
    {
        var text = new StringBuilder();

        text.Append($"Molim odobrenje zahtjeva \"{context.Title}\" iz kategorije {context.CategoryName}. ");

        text.Append(string.IsNullOrWhiteSpace(context.ReplacementEquipmentName)
            ? "Traženu opremu trenutno nemam zaduženu, a potrebna mi je za redovito obavljanje poslova radnog mjesta. "
            : $"Zahtjev se odnosi na zamjenu opreme {context.ReplacementEquipmentName}, koja više ne zadovoljava potrebe posla. ");

        text.Append("Nabavom bi se izbjegli zastoji u radu, a oprema bi se koristila isključivo u službene svrhe. ");
        text.Append("Molim da se zahtjev razmotri u okviru raspoloživih sredstava.");

        return Task.FromResult(Suggestion(text.ToString()));
    }

    private static string? MatchGroup(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static int MatchLookup(string lowerText, IReadOnlyList<LookupDto> options) =>
        options.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name)
                                 && lowerText.Contains(o.Name.ToLowerInvariant()))?.Id ?? 0;

    // Model je ono sto stoji odmah iza prepoznatog proizvodaca, do zareza ili kraja recenice.
    private static string? MatchModel(string text, string? manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
            return null;

        var match = Regex.Match(
            text,
            $@"{Regex.Escape(manufacturer)}\s+([A-Za-z0-9][A-Za-z0-9\- ]{{1,40}})",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.Trim(' ', ',', '.', ';') : null;
    }

    private static decimal? MatchValue(string text)
    {
        var raw = MatchGroup(text, @"(\d+(?:[.,]\d{1,2})?)\s*(?:eur|€|kn)")
               ?? MatchGroup(text, @"(?:vrijednost|cijena|nabavna)\s*[:\-]?\s*(\d+(?:[.,]\d{1,2})?)");

        if (raw is null)
            return null;

        return decimal.TryParse(
            raw.Replace(',', '.'),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;
    }

    private static string BuildName(EquipmentIntakeSuggestionDto suggestion, string text)
    {
        var parts = new[] { suggestion.Manufacturer, suggestion.Model }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        if (parts.Count > 0)
            return string.Join(" ", parts);

        // Bez proizvodaca i modela uzima se prvi dio biljeske kao radni naziv.
        var firstPart = text.Split(',', '.', '\n').FirstOrDefault()?.Trim() ?? string.Empty;

        return firstPart.Length > 150 ? firstPart[..150] : firstPart;
    }

    private static double Confidence(EquipmentIntakeSuggestionDto suggestion)
    {
        var filled = 0;

        if (!string.IsNullOrWhiteSpace(suggestion.InventoryNumber)) filled++;
        if (!string.IsNullOrWhiteSpace(suggestion.Manufacturer)) filled++;
        if (!string.IsNullOrWhiteSpace(suggestion.Model)) filled++;
        if (!string.IsNullOrWhiteSpace(suggestion.SerialNumber)) filled++;
        if (suggestion.PurchaseValue is not null) filled++;
        if (suggestion.EquipmentCategoryId > 0) filled++;
        if (suggestion.LocationId > 0) filled++;

        return Math.Round(filled / 7d, 2);
    }

    private static AiSuggestionDto Suggestion(string text) => new()
    {
        Text = text.Trim(),
        GeneratedAt = DateTime.UtcNow
    };

    private static string Items(int count) => count == 1
        ? "1 stavku"
        : $"{count} stavaka";

    private static string JoinCroatian(List<string> parts) => parts.Count switch
    {
        1 => parts[0],
        2 => $"{parts[0]} i {parts[1]}",
        _ => string.Join(", ", parts.Take(parts.Count - 1)) + $" i {parts[^1]}"
    };
}
