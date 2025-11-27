using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;

namespace diet_tracker_api.Services
{
    public class Auth0ManagementApiClient : IAuth0ManagementApiClient, IDisposable
    {
        private bool disposedValue;

        public ManagementApiClient Client { get; private set; }
        
        private Auth0ManagementApiClient(ManagementApiClient client)
        {
            Client = client;
        }

        public static async Task<Auth0ManagementApiClient> CreateAsync(string clientId, string clientSecret, string domain)
        {
            var authClient = new AuthenticationApiClient(domain);
            var response = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                Audience = $"https://{domain}/api/v2/",
                ClientId = clientId,
                ClientSecret = clientSecret,
                SigningAlgorithm = JwtSignatureAlgorithm.RS256,
            });

            var managementClient = new ManagementApiClient(response.AccessToken, domain);
            return new Auth0ManagementApiClient(managementClient);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Client?.Dispose();
                }

                Client = null;
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}