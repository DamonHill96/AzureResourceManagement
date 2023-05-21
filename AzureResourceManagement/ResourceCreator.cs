using Azure;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureResourceManagement.Configuration.ResourceManager;
using AzureResourceManagement.Contexts;
using AzureResourceManagement.Managers;
using AzureResourceManagement.Managers.StateManager;
using Microsoft.Extensions.Options;

namespace AzureResourceManagement;

public interface IResourceCreator
{
    Task<ResourceCreatorResult> ExecuteAsync();
}

internal class ResourceCreator : IResourceCreator
{
    private readonly ArmClient _armClient;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IStateManager _stateManager;
    private readonly ResourceManagerOptions _options;

    public ResourceCreator(ArmClient armClient, IOptions<ResourceManagerOptions> options, ISubscriptionManager subscriptionManager, IStateManager stateManager)
    {
        _armClient = armClient;
        _subscriptionManager = subscriptionManager;
        _stateManager = stateManager;
        options.Value.Validate();
        _options = options.Value;
    }

    public async Task<ResourceCreatorResult> ExecuteAsync()
    {
        var subscription = await _subscriptionManager.GetSubscriptionAsync(_options.SubscriptionId);
        var resourceGroup = await CreateResourceGroup(subscription);

        return await CreateResources(resourceGroup, subscription.Data);
    }

    private async Task<ResourceGroupResource> CreateResourceGroup(SubscriptionResource subscription)
    {
        var rgCollection = subscription.GetResourceGroups();

        var rgData = new ResourceGroupData(_options.ResourceGroupLocation);

        if (await rgCollection.ExistsAsync(_options.ResourceGroupName))
        {
            Console.WriteLine($"{_options.ResourceGroupName} already exists. Updating...");
        }
        else
        {
            Console.WriteLine($"{_options.ResourceGroupName} does not exist. Creating...");
        }

        var resourceGroup = await rgCollection.CreateOrUpdateAsync(WaitUntil.Completed, _options.ResourceGroupName, rgData);

        Console.WriteLine($"Successfully created resource group with name: {_options.ResourceGroupName}");

        return resourceGroup.Value;
    }

    private async Task<ResourceCreatorResult> CreateResources(ResourceGroupResource resourceGroupResource, SubscriptionData subscriptionData)
    {
        Console.WriteLine("Now creating resources...");

        var resourceDefinitions = GetResourceDefinitions(resourceGroupResource, subscriptionData);

        // todo
        if (true || _options.MultiThreadedCreate)
        {
            var resourceCreationTasks = resourceDefinitions.Values
                .Select(async instance => await CreateFromDefinition(instance));
        
            await Task.WhenAll(resourceCreationTasks);
        }
        else
        {
            foreach (var definitions in resourceDefinitions.Values)
            {
                await CreateFromDefinition(definitions);
            }
        }

        return new ResourceCreatorResult(
            _stateManager.GetCreated(),
            _stateManager.GetOutputs()
        );

        async Task CreateFromDefinition(AzureResourceDefinition instance)
        {
            var identifier = await instance.Create(_armClient);

            await instance.PostCreate(resourceGroupResource, identifier);

            Console.WriteLine($"Successfully created resource: {instance.ResourceName}");
        }
    }

    private Dictionary<Type, AzureResourceDefinition> GetResourceDefinitions(ResourceGroupResource resourceGroupResource, SubscriptionData subscriptionData)
    {
        var resourceDefinitions = DefinitionsHelper.GetAzureResourceDefinitions()
            .Select(t => new { Type = t, Instance = (AzureResourceDefinition)Activator.CreateInstance(t, CreateResourceContext(resourceGroupResource, subscriptionData, t))! })
            .ToDictionary(k => k.Type, v => v.Instance);

        var resourcesToSkip = GetResourcesToSkip(resourceDefinitions);

        foreach (var resource in resourcesToSkip)
        {
            resourceDefinitions.Remove(resource);
        }

        return resourceDefinitions;
    }

    private IEnumerable<Type> GetResourcesToSkip(Dictionary<Type, AzureResourceDefinition> resourceDefinitions)
    {
        var excludedResources = resourceDefinitions
            .Select(def => def.Key)
            .Where(IsExcluded)
            .ToList();

        var allResourcesToSkip = new List<Type>(excludedResources);

        Console.WriteLine($"Skipping resources: {string.Join(",", excludedResources.Select(x => x.Name))}");

        foreach (var skipped in excludedResources)
        {
            var resourcesDependantOnSkipped = GetResourcesDependantOnResource(skipped).ToList();

            Console.WriteLine($"The following resources require {skipped.Name} to be created and will also be skipped: {string.Join(",", resourcesDependantOnSkipped.Select(x => x.Name))}");

            allResourcesToSkip.AddRange(resourcesDependantOnSkipped);
        }

        return allResourcesToSkip.Distinct();

        IEnumerable<Type> GetResourcesDependantOnResource(Type resourceType)
        {
            return resourceDefinitions
                .Where(x => x.Value.DependsOn.Contains(resourceType))
                .Select(x => x.Key);
        }

        bool IsExcluded(Type defType)
        {
            return _options.ResourceOptions.GetOptionsForType(defType).DontCreateThisResource;
        }
    }

    private ResourceContext CreateResourceContext(ResourceGroupResource resourceGroupResource, SubscriptionData subscriptionData, Type type)
    {
        var variablesContext = new VariablesContext(subscriptionData, resourceGroupResource.Data);
        var variables = _options.VariablesFactory(variablesContext);

        return new ResourceContext(resourceGroupResource, _stateManager, _options.ResourceOptions.GetOptionsForType(type), variables)
        {
            Tags = _options.TagsFactory(variables)
        };
    }
}