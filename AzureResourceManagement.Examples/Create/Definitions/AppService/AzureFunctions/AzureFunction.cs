using Azure;
using Azure.Core;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Examples.Create.Definitions.AppService.AzureFunctions;

public class AzureFunction : AzureResourceDefinition
{
    public AzureFunction(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    public override IEnumerable<Type> DependsOn { get; } = new[] { typeof(Storage.Storage) };

    protected override async Task<ResourceIdentifier> Create()
    {
        WebSiteResource function = null;
        // todo getting 500 error, but the function still creates, so swallow error
        try
        {
            function = await CreateFunctions(ResourceContext, GetDependency(typeof(Storage.Storage)));
        }
        catch
        {
            // ignored
        }

        return function?.Id;
    }

    private async Task<WebSiteResource> CreateFunctions(ResourceContext context, ResourceIdentifier storageResourceIdentifier)
    {
        Console.WriteLine("Creating Functions");
        var data = new WebSiteData(AzureLocation.UKSouth)
        {
            Kind = "functionapp",
            Reserved = false,
            IsXenon = false,
            HyperV = false,
            SiteConfig = new SiteConfigProperties
            {
                NumberOfWorkers = 1,
                AcrUseManagedIdentityCreds = false,
                AlwaysOn = false,
                Http20Enabled = false,
                FunctionAppScaleLimit = 200,
                MinimumElasticInstanceCount = 0,
                NetFrameworkVersion = "v6.0",
                ManagedPipelineMode = ManagedPipelineMode.Integrated,
                LoadBalancing = SiteLoadBalancing.LeastRequests,
                PreWarmedInstanceCount = 0,
                
            },
            ScmSiteAlsoStopped = false,
            ClientAffinityEnabled = false,
            ClientCertEnabled = false,
            ClientCertMode = ClientCertMode.Required,
            HostNamesDisabled = false
        };

        data.SiteConfig
            .AddSiteConfig("AzureWebJobsStorage", await Helpers.GetStorageAccountConnectionStringAsync(context.ResourceGroup, storageResourceIdentifier))
            .AddSiteConfig("ClientId", context.GetVariable("ClientId"))
            .AddSiteConfig("ClientSecret", context.GetVariable("ClientSecret"));

        return (await context.ResourceGroup.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, "example-app-function", data)).Value;
    }
}