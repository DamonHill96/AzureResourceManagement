using Azure;
using Azure.Core;
using Azure.ResourceManager.Cdn;
using Azure.ResourceManager.Cdn.Models;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Examples.Create.Definitions.Cdn;

public class CdnEndpoint : AzureResourceDefinition
{
    public CdnEndpoint(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    public override IEnumerable<Type> DependsOn { get; } = new[] { typeof(Storage.Storage), typeof(CdnProfile) };

    private const string OriginName = "uiOrigin";
    
    protected override async Task<ResourceIdentifier> Create()
    {
        var cdnProfile = await ResourceContext.ResourceGroup.GetProfileAsync(GetDependency(typeof(CdnProfile)).Name);
        
        var origin = await CreateEndpoint(cdnProfile);

        return origin.Id;
    }

    private async Task<CdnEndpointResource> CreateEndpoint(ProfileResource cdnProfile)
    {
        Console.WriteLine("Creating Endpoint...");
        var storageAccountHostname = GetOutput(OutputKeys.StorageAccountHostName);
        const string endpointName = $"example-test";
        var endpointData = new CdnEndpointData(AzureLocation.NorthEurope)
        {
            IsHttpAllowed = true,
            IsHttpsAllowed = true,
            OptimizationType = OptimizationType.GeneralWebDelivery,
            OriginPath = $"/{Constants.StorageContainerName}"
        };
        
        var deepCreatedOrigin = new DeepCreatedOrigin(OriginName)
        {
            HostName = storageAccountHostname,
            OriginHostHeader = storageAccountHostname,
            Priority = 1,
            Weight = 1000
        };
        endpointData.Origins.Add(deepCreatedOrigin);
        var endpoints = cdnProfile.GetCdnEndpoints();

        // If exists, then just purge the endpoint and return
        if (await endpoints.ExistsAsync(endpointName))
        {
            Console.WriteLine("Endpoint already exists. Purging...");
            var o = (await endpoints.GetAsync(endpointName)).Value;
            // Should be safe to let this happen in the background
            await o.PurgeContentAsync(WaitUntil.Started, new PurgeContent(new []{"/*"}));
            return o;
        }
        
        var origin = (await cdnProfile.GetCdnEndpoints().CreateOrUpdateAsync(WaitUntil.Completed, endpointName, endpointData)).Value;

        return origin;
    }
}