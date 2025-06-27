using Money_Spending_Tracker.Features.Storage;
using System.Text;

namespace Money_Spending_Tracker.Features.Api;

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
        SpectacularJWTRefresh jwtRefresh = await apiClient.Get_a_new_access_tokenAsync(
            new JWTRefreshRequest()
            {
                Refresh = apiToken.RefreshToken,
            }
        );

        apiToken.UpdateToken(jwtRefresh.Access, jwtRefresh.Access_expires);
        await ApiToken.SetToStorageAsync(apiToken);

        return apiToken.AccessToken;
    }

    private async Task<string> ReAuthenticate(HttpClient client)
    {
        Client apiClient = new(client);

        var requestBody = new JWTObtainPairRequest()
        {
            Secret_id = await SecureStorage.Default.GetAsync(StorageKeys.API_SECRET_ID),
            Secret_key = await SecureStorage.Default.GetAsync(StorageKeys.API_SECRET_KEY),
        };

        SpectacularJWTObtain jwt = await apiClient.Obtain_new_access_refresh_token_pairAsync(requestBody);

        ApiToken apiToken = new(
            jwt.Access,
            DateTime.Now.AddSeconds(jwt.Access_expires),
            jwt.Refresh,
            DateTime.Now.AddSeconds(jwt.Refresh_expires)
        );

        await ApiToken.SetToStorageAsync(apiToken);

        return apiToken.AccessToken;
    }
}
