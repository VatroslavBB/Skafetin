using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Skafetin.Shared.DTOs;

namespace Skafetin.Api.Ai;

public sealed class OpenAiService : IAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string IntakeSchema = """
    {
      "type": "object",
      "properties": {
        "Name": { "type": "string" },
        "InventoryNumber": { "type": "string" },
        "Manufacturer": { "type": "string" },
        "Model": { "type": "string" },
        "SerialNumber": { "type": "string" },
        "PurchaseValue": { "type": ["number", "null"] },
        "EquipmentCategoryId": { "type": "integer" },
        "LocationId": { "type": "integer" },
        "Description": { "type": "string" },
        "Confidence": { "type": "number" },
        "Warnings": { "type": "array", "items": { "type": "string" } }
      },
      "required": [
        "Name", "InventoryNumber", "Manufacturer", "Model", "SerialNumber",
        "PurchaseValue", "EquipmentCategoryId", "LocationId", "Description",
        "Confidence", "Warnings"
      ],
      "additionalProperties": false
    }
    """;

    private const string CheckSchema = """
    {
      "type": "object",
      "properties": {
        "IsReady": { "type": "boolean" },
        "Warnings": { "type": "array", "items": { "type": "string" } },
        "Summary": { "type": "string" }
      },
      "required": ["IsReady", "Warnings", "Summary"],
      "additionalProperties": false
    }
    """;

    private readonly ChatClient _client;
    private readonly AiOptions _options;

    public OpenAiService(IOptions<AiOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "Ai:ApiKey nije postavljen. Postavi ga naredbom: " +
                "dotnet user-secrets set \"Ai:ApiKey\" \"<kljuc>\" u mapi projekta Skafetin.Api.");

        if (string.IsNullOrWhiteSpace(_options.Model))
            throw new InvalidOperationException("Ai:Model nije postavljen.");

        _client = new ChatClient(_options.Model, _options.ApiKey);
    }

    public string ProviderName => "OpenAI";
    public string ModelName => _options.Model;
    public bool UsesExternalService => true;

    public async Task<AiSuggestionDto> SummarizeInventoryAsync(
        InventorySummaryContext context,
        CancellationToken cancellationToken = default)
    {
        var text = await CompleteTextAsync(
            AiPrompts.InventorySummary,
            JsonSerializer.Serialize(context),
            cancellationToken);

        return new AiSuggestionDto { Text = text, GeneratedAt = DateTime.UtcNow };
    }

    public async Task<AiSuggestionDto> DraftRequestAsync(
        RequestDraftContext context,
        CancellationToken cancellationToken = default)
    {
        var text = await CompleteTextAsync(
            AiPrompts.RequestDraft,
            JsonSerializer.Serialize(context),
            cancellationToken);

        return new AiSuggestionDto { Text = text, GeneratedAt = DateTime.UtcNow };
    }

    public async Task<EquipmentIntakeSuggestionDto> SuggestEquipmentIntakeAsync(
        EquipmentIntakeContext context,
        CancellationToken cancellationToken = default)
    {
        var input = JsonSerializer.Serialize(new
        {
            Biljeska = context.Text,
            Kategorije = context.Categories,
            Lokacije = context.Locations
        });

        var result = await CompleteStructuredAsync<EquipmentIntakeSuggestionDto>(
            AiPrompts.EquipmentIntake, input, "equipment_intake", IntakeSchema, cancellationToken);

        return Validate(result, context);
    }

    public async Task<EquipmentDataCheckDto> CheckEquipmentDataAsync(
        EquipmentCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await CompleteStructuredAsync<EquipmentDataCheckDto>(
            AiPrompts.EquipmentCheck,
            JsonSerializer.Serialize(context),
            "equipment_check",
            CheckSchema,
            cancellationToken);

        // Model zna proturjeciti sam sebi; oznaka spremnosti se izvodi iz upozorenja.
        result.IsReady = result.Warnings.Count == 0;

        return result;
    }

    private async Task<string> CompleteTextAsync(
        string systemPrompt,
        string input,
        CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(input)
        ];

        var completion = await _client.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var text = completion.Value.Content.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("AI servis nije vratio tekstualni odgovor.");

        return text.Trim();
    }

    private async Task<T> CompleteStructuredAsync<T>(
        string systemPrompt,
        string input,
        string schemaName,
        string schema,
        CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(input)
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: schemaName,
                jsonSchema: BinaryData.FromString(schema),
                jsonSchemaIsStrict: true)
        };

        var completion = await _client.CompleteChatAsync(messages, options, cancellationToken);
        var json = completion.Value.Content.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("AI servis nije vratio strukturirani odgovor.");

        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException("Strukturirani odgovor nije u očekivanom obliku.");
    }

    // Strukturirani odgovor se ponovno provjerava kao obican DTO
    private static EquipmentIntakeSuggestionDto Validate(
        EquipmentIntakeSuggestionDto suggestion,
        EquipmentIntakeContext context)
    {
        if (context.Categories.All(c => c.Id != suggestion.EquipmentCategoryId))
        {
            suggestion.EquipmentCategoryId = 0;
            suggestion.Warnings.Add("Kategorija nije prepoznata - odaberi je iz popisa.");
        }

        if (context.Locations.All(l => l.Id != suggestion.LocationId))
        {
            suggestion.LocationId = 0;
            suggestion.Warnings.Add("Lokacija nije prepoznata - odaberi je iz popisa.");
        }

        if (suggestion.PurchaseValue < 0)
        {
            suggestion.PurchaseValue = null;
            suggestion.Warnings.Add("Nabavna vrijednost nije prepoznata.");
        }

        suggestion.Confidence = Math.Clamp(suggestion.Confidence, 0, 1);

        if (suggestion.Name.Length > 150)
            suggestion.Name = suggestion.Name[..150];

        if (suggestion.InventoryNumber.Length > 30)
            suggestion.InventoryNumber = suggestion.InventoryNumber[..30];

        suggestion.Warnings.Add("Prijedlog je nastao vanjskom uslugom; provjeri svako polje prije spremanja.");

        return suggestion;
    }
}
