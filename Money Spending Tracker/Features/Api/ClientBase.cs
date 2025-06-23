using Money_Spending_Tracker.Features.Api;
using Money_Spending_Tracker.Features.Storage;
using System.Text;

namespace Money_Spending_Tracker.BankAccountDataApi;

partial class Client
{
    private static partial Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response, CancellationToken cancellationToken);

    private static partial Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        //Implementation needed to adhere to generated code contract.
        return Task.CompletedTask;
    }

    private partial Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken cancellationToken);

    private partial async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken cancellationToken)
    {
        await PrepareRequestAsync(client, request, urlBuilder.ToString(), cancellationToken);
    }

    private partial Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url, CancellationToken cancellationToken);

    private partial async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url, CancellationToken cancellationToken)
    {
        if (url.Contains("/token/"))
        {
            //This is a token request, we can skip the authentication check
            return;
        }

        string accessToken = await GetAccessToken(client);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }

    private async Task<string> GetAccessToken(HttpClient client)
    {
        ApiToken? apiToken = await ApiToken.GetFromStorageAsync();

        if (apiToken == null)
        {
            return await ReAuthenticate(client);
        }

        if (apiToken.IsExpired)
        {
            return await RefreshAccessToken(client);
        }

        return apiToken.AccessToken;
    }

    private async Task<string> RefreshAccessToken(HttpClient client)
    {
        ApiToken? apiToken = await ApiToken.GetFromStorageAsync();

        if (apiToken == null)
        {
            return await ReAuthenticate(client);
        }

        if (apiToken.IsRefreshExpired)
        {
            return await ReAuthenticate(client);
        }

        Client apiClient = new(client);
        SpectacularJWTRefresh jwtRefresh = await apiClient.ApiV2TokenRefreshAsync(new() { Refresh = apiToken.RefreshToken });

        apiToken.UpdateToken(jwtRefresh.Access, jwtRefresh.Access_expires);
        await ApiToken.SetToStorageAsync(apiToken);

        return apiToken.AccessToken;
    }

    private async Task<string> ReAuthenticate(HttpClient client)
    {
        Client apiClient = new(client);

        SpectacularJWTObtain jwt = await apiClient.ApiV2TokenNewAsync(new()
        {
            Secret_id = await SecureStorage.Default.GetAsync(StorageKeys.API_SECRET_ID),
            Secret_key = await SecureStorage.Default.GetAsync(StorageKeys.API_SECRET_KEY),
        });

        ApiToken apiToken = new(jwt.Access, jwt.Access_expires, jwt.Refresh, jwt.Refresh_expires);
        await ApiToken.SetToStorageAsync(apiToken);

        return apiToken.AccessToken;
    }
}
