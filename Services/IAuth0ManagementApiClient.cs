using Auth0.ManagementApi;

namespace diet_tracker_api.Services
{
    public interface IAuth0ManagementApiClient
    {
        ManagementApiClient Client { get; }
    }
}