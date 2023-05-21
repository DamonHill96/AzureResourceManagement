using Ardalis.GuardClauses;
using Azure.Core;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Configuration.ResourceManager;

public class ResourceManagerOptions
{
    public string? ResourceGroupName { get; set; }
    public string? SubscriptionId { get; set; }
    public AzureLocation ResourceGroupLocation { get; set; }
    public Func<Variables, Dictionary<string, string>> TagsFactory { get; set; } = _ => new Dictionary<string, string>();
    public Func<VariablesContext, Variables> VariablesFactory = _ => new Variables();

    // todo, it'll all break if false!
    internal bool MultiThreadedCreate = true;

    public ResourceCreationConfiguration ResourceOptions { get; } = new();
    
    public void Validate()
    {
        Guard.Against.NullOrWhiteSpace(ResourceGroupName, nameof(ResourceGroupName));
        Guard.Against.Null(ResourceGroupLocation, nameof(ResourceGroupLocation));
    }
}