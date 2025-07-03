using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendDotnet.Services;

record class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class TokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private TokenResponse? _tokenCache;

    public TokenService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_tokenCache != null && !string.IsNullOrEmpty(_tokenCache.AccessToken))
            return _tokenCache.AccessToken;

        var realm = "fintatech";
        var tokenUrl = $"{_configuration["Auth:Uri"]}/identity/realms/{realm}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(
        [
            new("grant_type", "password"),
            new("client_id", "app-cli"),
            new("username", _configuration["Auth:Username"]),
            new("password", _configuration["Auth:Password"])
        ]);

        var response = await _httpClient.PostAsync(tokenUrl, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        _tokenCache = JsonSerializer.Deserialize<TokenResponse>(json);

        Console.WriteLine("Access token:");
        Console.WriteLine(_tokenCache?.AccessToken);

        return _tokenCache?.AccessToken;
    }
}
