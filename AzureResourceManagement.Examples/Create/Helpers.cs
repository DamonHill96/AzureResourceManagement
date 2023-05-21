using Azure.Core;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;

namespace AzureResourceManagement.Examples.Create;

public static class Helpers
{
    public static async Task<string> GetStorageAccountConnectionStringAsync(ResourceGroupResource resourceGroup, ResourceIdentifier storageResourceIdentifier)
    {
        if (storageResourceIdentifier.ResourceType.Type != "storageAccounts")
        {
            throw new Exception("Can only use a storage account resource identifier.");
        }

        var storageResource = (await resourceGroup.GetStorageAccountAsync(storageResourceIdentifier.Name)).Value;
        var storageKey = (await storageResource.GetKeysAsync()).Value.Keys[0].Value;

        return $"DefaultEndpointsProtocol=https;AccountName={storageResource.Data.Name};AccountKey={storageKey};EndpointSuffix=core.windows.net";
    }

    public static SiteConfigProperties AddSiteConfig(this SiteConfigProperties props, string key, string value)
    {
        props.AppSettings.Add(new NameValuePair
        {
            Name = key,
            Value = value
        });
        
        return props;
    }
}