using Azure;
using Azure.Core;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Examples.Create.Definitions.AppService;

public class WebApp : AzureResourceDefinition
{
    public WebApp(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    public override IEnumerable<Type> DependsOn { get; } = new[] { typeof(AppServicePlan) };

    protected override async Task<ResourceIdentifier> Create()
    {
        var app = await CreateWebApp(ResourceContext.ResourceGroup);
    
        return app.Id;
    }

    private async Task<WebSiteResource> CreateWebApp(ResourceGroupResource resourceGroup)
    {
        Console.WriteLine("Creating Web App");
        var data = new WebSiteData(AzureLocation.UKSouth)
        {
            SiteConfig = new SiteConfigProperties()
        };
        
        data.SiteConfig.AppSettings.Add(new NameValuePair
        {
            Name = "ASPNETCORE_ENVIRONMENT",
            Value = "Staging"
        });
        
        return (await resourceGroup.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, "example-test", data)).Value;
    }
}