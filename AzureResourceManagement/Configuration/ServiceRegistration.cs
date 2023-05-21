using Azure.Identity;
using Azure.ResourceManager;
using AzureResourceManagement.Configuration.ResourceManager;
using AzureResourceManagement.Managers;
using AzureResourceManagement.Managers.ResourceManager;
using AzureResourceManagement.Managers.StateManager;
using Microsoft.Extensions.DependencyInjection;

namespace AzureResourceManagement.Configuration;

public static class ServiceRegistration
{
    public static IServiceCollection AddAzureResourceManagement(this IServiceCollection serviceCollection, Action<ResourceManagerOptions> optionsConfig)
    {
        serviceCollection.AddSingleton<ISubscriptionManager, SubscriptionManager>();
        serviceCollection.AddSingleton<IResourceManager, Managers.ResourceManager.ResourceManager>();
        serviceCollection.AddSingleton<IStateManager, InMemoryStateManager>();
        serviceCollection.AddSingleton<IResourceCreator, ResourceCreator>();
        serviceCollection.Configure(optionsConfig);
        serviceCollection.AddSingleton(_ => new ArmClient(new DefaultAzureCredential()));
        return serviceCollection;
    }
}