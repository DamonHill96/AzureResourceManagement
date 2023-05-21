using Azure;
using Azure.Core;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using AzureResourceManagement.Contexts;
using AzureResourceManagement.Examples.Create.Definitions.AppService.AzureFunctions;

namespace AzureResourceManagement.Examples.Create.Definitions.AppService;

public class AppServicePlan : AzureResourceDefinition
{
    public AppServicePlan(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    public override IEnumerable<Type> DependsOn { get; } = new[] { typeof(AzureFunction) };

    protected override async Task<ResourceIdentifier> Create()
    {
        var plan = await CreateAppServicePlan(ResourceContext.ResourceGroup);
        
        return plan.Id;
    }

    private async Task<AppServicePlanResource> CreateAppServicePlan(ResourceGroupResource resourceGroup)
    {
        Console.WriteLine("Creating App Service Plan");

        var data = new AppServicePlanData(AzureLocation.UKSouth)
        {
            Sku = new SkuDescription
            {
                Capacity = 0,
                Name = "F1",
                Tier = "Free",
                Size = "F1",
                Family = "F"
            },
            PerSiteScaling = false,
            ElasticScaleEnabled = false,
            MaximumElasticWorkerCount = 1,
            IsSpot = false,
            Reserved = false,
            IsXenon = false,
            HyperV = false,
            TargetWorkerCount = 0,
            TargetWorkerSizeId = 0,
            ZoneRedundant = false,
            Kind = "app"
        };

        return (await resourceGroup.GetAppServicePlans().CreateOrUpdateAsync(WaitUntil.Completed, "ASP-example-app-plan", data)).Value;
    }
}