using Azure.ResourceManager.Resources;
using AzureResourceManagement.Configuration.ResourceManager;
using AzureResourceManagement.Managers.StateManager;

namespace AzureResourceManagement.Contexts;

public class ResourceContext
{
    private readonly Variables _variables;

    public ResourceGroupResource ResourceGroup { get; }
    public Dictionary<string, string> Tags { get; internal init; } = new();

    internal readonly IStateManager StateManager;
    internal readonly ResourceOptions ResourceOptions;

    internal ResourceContext(ResourceGroupResource resourceGroup, IStateManager stateManager, ResourceOptions optionsForResource, Variables? variables = null)
    {
        ResourceGroup = resourceGroup;
        StateManager = stateManager;
        ResourceOptions = optionsForResource;
        _variables = variables ?? new Variables();
    }

    public string GetVariable(string key) => _variables.GetVariable(key);
}