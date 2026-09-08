using System.Text;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Ai;

public class MockAiService : IAiService
{
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
