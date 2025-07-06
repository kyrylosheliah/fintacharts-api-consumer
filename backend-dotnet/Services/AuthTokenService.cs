using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendDotnet.Services;

record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonIgnore]
    public DateTime ExpiryTime { get; set; } = DateTime.MinValue;
}

public class AuthTokenService(IHttpClientFactory httpClientFactory, IConfiguration _configuration)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private TokenResponse? _tokenCache = null;
    private readonly string _tokenUrl = $"{_configuration["Base:Uri"]}/identity/realms/{_configuration["Auth:Realm"]}/protocol/openid-connect/token";

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_tokenCache != null)
        {
            Console.WriteLine("### token cached");
            Console.WriteLine(_tokenCache.ExpiresIn);
            Console.WriteLine(_tokenCache.ExpiryTime);
            if (_tokenCache.ExpiryTime > DateTime.UtcNow.AddMinutes(1))
            {
                //&& !string.IsNullOrEmpty(_tokenCache.AccessToken)
                return _tokenCache.AccessToken;
            }
            if (!string.IsNullOrEmpty(_tokenCache.RefreshToken))
            {
                var refresh_success = await TryRefreshTokenAsync(_tokenCache.RefreshToken);
                if (refresh_success)
                {
                    return _tokenCache?.AccessToken;
                }
            }
        }

        var request_success = await TryRequestTokenAsync();
        if (request_success)
        {
            return _tokenCache?.AccessToken;
        }

        return null;
    }

    private async Task<bool> TryRequestTokenAsync()
    {
        var content = new FormUrlEncodedContent(
        [
            new("grant_type", "password"),
            new("client_id", "app-cli"),
            new("username", _configuration["Auth:Username"]),
            new("password", _configuration["Auth:Password"])
        ]);

        var response = await _httpClient.PostAsync(_tokenUrl, content);
        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        _tokenCache = JsonSerializer.Deserialize<TokenResponse>(json);

        return true;
    }

    private async Task<bool> TryRefreshTokenAsync(string refreshToken)
    {
        var content = new FormUrlEncodedContent(
        [
            new("grant_type", "refresh_token"),
            new("client_id", "app-cli"),
            new("refresh_token", refreshToken)
        ]);

        var response = await _httpClient.PostAsync(_tokenUrl, content);
        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        var newToken = JsonSerializer.Deserialize<TokenResponse>(json);
        if (newToken == null)
            return false;

        newToken.ExpiryTime = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn);
        Console.WriteLine("### ExpiryTime");
        Console.WriteLine(newToken.ExpiryTime);
        Console.WriteLine(newToken.ExpiresIn);
        _tokenCache = newToken;

        return true;
    }
}
