namespace AzureResourceManagement.Managers.ResourceManager;

public class ResourceManagerResult
{
    public ResourceManagerResult(IEnumerable<AzureResource> azureResources)
    {
        AzureResources = azureResources.ToList();
    }

    public List<AzureResource> AzureResources { get; set; }
}