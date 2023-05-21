using Azure.Core;
using Azure.ResourceManager.Resources;

namespace AzureResourceManagement.Managers.ResourceManager;

public class AzureResource
{
    public AzureResource(ResourceIdentifier resourceIdentifier, GenericResourceData data)
    {
        ResourceIdentifier = resourceIdentifier;
        Data = data;
    }

    public ResourceIdentifier ResourceIdentifier { get; set; }
    public GenericResourceData Data { get; set; }
}