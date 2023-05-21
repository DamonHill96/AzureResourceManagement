using Azure;
using Azure.Core;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Cdn.Models;
using Azure.ResourceManager.Resources;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Examples.Create.Definitions.Cdn;

public class CdnProfile : AzureResourceDefinition
{
    public CdnProfile(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    protected override async Task<ResourceIdentifier> Create()
    {
        var profile = await CreateCdnProfile(ResourceContext.ResourceGroup);

        return profile.Id;
    }

    private async Task<ProfileResource> CreateCdnProfile(ResourceGroupResource resourceGroup)
    {
        Console.WriteLine("Creating CDN Profile...");

        var profileData = new ProfileData(AzureLocation.NorthEurope, new CdnSku { Name = CdnSkuName.StandardMicrosoft });

        return (await resourceGroup.GetProfiles().CreateOrUpdateAsync(WaitUntil.Completed, "example-app-CDN", profileData)).Value;
    }
}