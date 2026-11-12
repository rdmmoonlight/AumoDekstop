using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AumoDekstop.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        return await _httpClient.GetFromJsonAsync<T>(endpoint);
    }

    // Contoh metode POST ringan
    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T payload)
    {
        return await _httpClient.PostAsJsonAsync(endpoint, payload);
    }
}