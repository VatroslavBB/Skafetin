using System.Net;
using Skafetin.Shared.DTOs;

namespace Skafetin.App.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }
    public Task<ApiResult<T>> GetAsync<T>(string url)
        => SendAsync<T>(() => _http.GetAsync(url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object body)
        => SendAsync<T>(() => _http.PostAsJsonAsync(url, body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object body)
        => SendAsync<T>(() => _http.PutAsJsonAsync(url, body));

    public Task<ApiResult<bool>> DeleteAsync(string url)
        => SendAsync<bool>(() => _http.DeleteAsync(url));

    public Task<ApiResult<T>> PostFormAsync<T>(string url, MultipartFormDataContent content)
        => SendAsync<T>(() => _http.PostAsync(url, content));

    public async Task<ApiResult<byte[]>> GetBytesAsync(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return ApiResult<byte[]>.Fail(await ReadErrorAsync(response));

            return ApiResult<byte[]>.Ok(await response.Content.ReadAsByteArrayAsync());
        }
        catch (HttpRequestException)
        {
            return ApiResult<byte[]>.Fail("Poslužitelj nije dostupan. Provjeri je li Api projekt pokrenut.");
        }
    }

    private async Task<ApiResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> call)
    {
        try
        {
            var response = await call();

            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Fail(await ReadErrorAsync(response));

            if (response.StatusCode == HttpStatusCode.NoContent ||
                response.Content.Headers.ContentLength == 0)
                return ApiResult<T>.Ok(default);

            return ApiResult<T>.Ok(await response.Content.ReadFromJsonAsync<T>());
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail("Poslužitelj nije dostupan. Provjeri je li Api projekt pokrenut.");
        }
        catch (NotSupportedException)
        {
            return ApiResult<T>.Fail("Odgovor poslužitelja nije u očekivanom obliku.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
            if (!string.IsNullOrWhiteSpace(error?.Message))
                return error.Message;
        }
        catch
        {
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Prijava je istekla ili nije izvršena.",
            HttpStatusCode.Forbidden => "Nemaš ovlasti za ovu radnju.",
            HttpStatusCode.NotFound => "Traženi zapis ne postoji.",
            HttpStatusCode.BadRequest => "Zahtjev nije ispravan.",
            _ => "Došlo je do pogreške u komunikaciji s poslužiteljem."
        };
    }
}

