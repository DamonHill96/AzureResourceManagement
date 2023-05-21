using Azure.Core;

namespace AzureResourceManagement;

public class ResourceCreatorResult
{
    public ResourceCreatorResult(Dictionary<string, ResourceIdentifier> createdResources, Dictionary<string, string> outputs)
    {
        CreatedResources = createdResources;
        Outputs = outputs;
    }

    public Dictionary<string, ResourceIdentifier> CreatedResources { get; }
    public Dictionary<string, string> Outputs { get; }
}