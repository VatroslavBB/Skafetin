using MudBlazor;

namespace Skafetin.App.Components.Shared;

public static class DialogExtensions
{
    public static async Task<bool> ConfirmDeleteAsync(
        this IDialogService dialogService,
        string what,
        string? title = null)
    {
        var confirmed = await dialogService.ShowMessageBoxAsync(
            title: title ?? "Potvrda brisanja",
            message: $"{what} bit će trajno obrisano. Želiš li nastaviti?",
            yesText: "Obriši",
            cancelText: "Odustani");

        return confirmed == true;
    }
}

