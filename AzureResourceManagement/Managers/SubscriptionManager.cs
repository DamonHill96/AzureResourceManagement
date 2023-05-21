using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

namespace AzureResourceManagement.Managers;

internal class SubscriptionManager : ISubscriptionManager
{
    private readonly ArmClient _armClient;

    public SubscriptionManager(ArmClient armClient)
    {
        _armClient = armClient;
    }

    public async Task<SubscriptionResource> GetSubscriptionAsync(string? subscriptionId)
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            return await _armClient.GetDefaultSubscriptionAsync();
        }

        var getSubscriptionResponse = await _armClient.GetSubscriptions().GetAsync(subscriptionId);

        if (getSubscriptionResponse.GetRawResponse().IsError)
        {
            throw new Exception($"Failed to get subscription with id {subscriptionId}");
        }

        return getSubscriptionResponse.Value;
    }
}

public interface ISubscriptionManager
{
    public Task<SubscriptionResource> GetSubscriptionAsync(string? subscriptionId);
}