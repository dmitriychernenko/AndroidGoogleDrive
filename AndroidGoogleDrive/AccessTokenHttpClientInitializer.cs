using System.Net.Http.Headers;
using Google.Apis.Http;

namespace AndroidGoogleDrive
{
    public class AccessTokenHttpClientInitializer : IConfigurableHttpClientInitializer
    {
        private readonly string _accessToken;

        public AccessTokenHttpClientInitializer(string accessToken)
        {
            _accessToken = accessToken;
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
    }
}