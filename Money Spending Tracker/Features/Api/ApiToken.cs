using Money_Spending_Tracker.Features.Storage;
using System.Text.Json;

namespace Money_Spending_Tracker.Features.Api;

internal class ApiToken
{
    public string AccessToken { get; private set; }

    public DateTime ExpireDateTime { get; private set; }

    public bool IsExpired { get => DateTime.Now.AddMinutes(-30) >= ExpireDateTime; }

    public string RefreshToken { get; private set; }

    public DateTime RefreshExpireDateTime { get; private set; }

    public bool IsRefreshExpired { get => DateTime.Now.AddMinutes(-30) >= ExpireDateTime; }

    public ApiToken(string accessToken, int expiresIn, string refreshToken, int refreshExpiresIn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        ExpireDateTime = DateTime.Now.AddSeconds(expiresIn);
        RefreshExpireDateTime = DateTime.Now.AddSeconds(refreshExpiresIn);
    }

    public void UpdateToken(string accessToken, int expiresIn)
    {
        AccessToken = accessToken;
        ExpireDateTime = DateTime.Now.AddSeconds(expiresIn);
    }

    public static async Task<ApiToken?> GetFromStorageAsync()
    {
        string? apiTokenJson = await SecureStorage.Default.GetAsync(StorageKeys.API_TOKEN);
        if (string.IsNullOrEmpty(apiTokenJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ApiToken>(apiTokenJson);
    }

    public static async Task SetToStorageAsync(ApiToken apiToken)
    {
        await SecureStorage.Default.SetAsync(StorageKeys.API_TOKEN, JsonSerializer.Serialize(apiToken).ToString());
    }
}
