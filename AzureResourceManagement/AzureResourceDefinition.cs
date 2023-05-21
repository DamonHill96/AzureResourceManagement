using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using AzureResourceManagement.Configuration.ResourceManager;
using AzureResourceManagement.Contexts;
using AzureResourceManagement.Extensions;
using AzureResourceManagement.Managers.StateManager;

namespace AzureResourceManagement;

public abstract class AzureResourceDefinition
{
    protected readonly ResourceContext ResourceContext;
    private IStateManager StateManager => ResourceContext.StateManager;
    private ResourceOptions ResourceOptions => ResourceContext.ResourceOptions;

    protected AzureResourceDefinition(ResourceContext resourceContext)
    {
        ResourceContext = resourceContext;
    }

    public virtual string ResourceName => GetType().Name;
    public virtual IEnumerable<Type> DependsOn { get; } = Type.EmptyTypes;
    private Dictionary<Type, ResourceIdentifier> Dependencies => StateManager!.GetResourceIdentifiersForDependencies(this);

    public async Task<ResourceIdentifier> Create(ArmClient client)
    {
        ValidateDependencySetup();

        await WaitForDependencies();

        Console.WriteLine($"Creating resource: {ResourceName}");

        var identifier = await Create();

        await SetResourceTags(identifier);

        StateManager.Add(GetType(), identifier);
        
        return identifier;
    }

    private async Task WaitForDependencies()
    {
        // If there are dependencies that haven't yet been added into the state manager
        // Then don't create the resource until they're finished
        while (DependsOn.Any() && DependsOn.Any(y => Dependencies.All(z => z.Key != y)))
        {
            await Task.Delay(500);
        }
    }

    private void ValidateDependencySetup()
    {
        foreach (var dependencyType in DependsOn)
        {
            // If the types added to DependsOn don't inherit from AzureResourceDefinition
            // Then throw, otherwise if we allow this we'll be waiting for a dependency that will never be resolved
            if (!dependencyType.IsAzureResourceDefinition())
            {
                throw new Exception($"The type {dependencyType.Name} does not inherit from {nameof(AzureResourceDefinition)}, and as such cannot be added to the DependsOn collection");
            }

            if (dependencyType == GetType())
            {
                throw new Exception("DependsOn cannot contain the type of the overriding class");
            }
        }
    }

    protected abstract Task<ResourceIdentifier> Create();

    /// <summary>
    /// Run some code after the creation of the resource
    /// </summary>
    /// <returns></returns>
    protected internal virtual Task<bool> PostCreate(ResourceGroupResource resourceGroup, ResourceIdentifier createdResource)
    {
        return Task.FromResult(true);
    }

    protected ResourceIdentifier GetDependency(Type type)
    {
        if (!DependsOn.Contains(type))
        {
            throw new Exception($"{type.Name} is not a dependency of {ResourceName}. Has it been added to DependsOn?");
        }

        return Dependencies[type];
    }
    
    protected void OutputToApplication(string key, string value)
    {
        StateManager.AddOutput(key, value);
    }

    /// <summary>
    /// Gets a specific output, that has been added via the OutputToApplication method.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected string GetOutput(string key)
    {
        return StateManager.GetOutput(key);
    }
    
    private async Task SetResourceTags(ResourceIdentifier identifier)
    {
        if (ResourceOptions.DontWriteTagsOnCreate)
        {
            return;
        }
        
        if (ResourceContext.ResourceGroup.GetGenericResources().FirstOrDefault(x => x.Id == identifier) is { } resource)
        {
            await resource.SetTagsAsync(ResourceContext.Tags);
        }
    }
}