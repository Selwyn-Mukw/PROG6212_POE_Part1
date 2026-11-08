using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace RaceDay.Mvc.Services;

public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RaceDayApiClient(
        IHttpClientFactory factory,
        IHttpContextAccessor accessor)
    {
        _factory = factory;
        _httpContextAccessor = accessor;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient("RaceDayApi");
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);

        string? token = _httpContextAccessor.HttpContext?
            .Session.GetString("JwtToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    // Send JSON requests
    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string url, object? body = null)
    {
        var client = CreateClient();

        using var request = CreateRequest(method, url);

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request);
    }

    // Upload files using multipart/form-data
    public async Task<HttpResponseMessage> UploadFileAsync(
        string url, IFormFile file)
    {
        var client = CreateClient();

        using var request = CreateRequest(HttpMethod.Post, url);

        using var content = new MultipartFormDataContent();

        using var stream = file.OpenReadStream();

        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        request.Content = content;

        return await client.SendAsync(request);
    }

    // Read JSON responses
    public async Task<T?> ReadAsync<T>(
        HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return default;

        string json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(
            json, JsonOptions);
    }
}