using Azure.ResourceManager;
using AzureResourceManagement.Configuration.ResourceManager;

namespace AzureResourceManagement.Managers.ResourceManager;

public class ResourceManager : IResourceManager
{
    private readonly ArmClient _armClient;
    private readonly ResourceManagerOptions _options;
    private readonly ISubscriptionManager _subscriptionManager;

    public ResourceManager(ArmClient armClient, ResourceManagerOptions options, ISubscriptionManager subscriptionManager)
    {
        _armClient = armClient;
        options.Validate();
        _options = options;
        _subscriptionManager = subscriptionManager;
    }

    public async Task<ResourceManagerResult> GetCreatedResourcesAsync()
    {
        var subscription = await _subscriptionManager.GetSubscriptionAsync(_options.SubscriptionId);
        var resourceGroup = await subscription.GetResourceGroupAsync(_options.ResourceGroupName);
        var resources = resourceGroup.Value.GetGenericResources();

        return new ResourceManagerResult(
            resources.Select(r => new AzureResource(
                r.Id,
                r.HasData ? r.Data : r.Get().Value.Data
            ))
        );
    }
}

public interface IResourceManager
{
    public Task<ResourceManagerResult> GetCreatedResourcesAsync();
}