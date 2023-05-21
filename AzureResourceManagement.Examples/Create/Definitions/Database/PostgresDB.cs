using System.Net;
using Azure;
using Azure.Core;
using Azure.ResourceManager.PostgreSql;
using Azure.ResourceManager.PostgreSql.Models;
using AzureResourceManagement.Contexts;

namespace AzureResourceManagement.Examples.Create.Definitions.Database;

public class PostgresDB : AzureResourceDefinition
{
    public PostgresDB(ResourceContext resourceContext) : base(resourceContext)
    {
    }

    protected override async Task<ResourceIdentifier> Create()
    {
        var props = new PostgreSqlServerPropertiesForDefaultCreate("postgres", "password&1");
        var opts = new PostgreSqlServerCreateOrUpdateContent(props, AzureLocation.UKSouth)
        {
            Sku = new PostgreSqlSku("GP_Gen5_4")
        };
        
        
        var server = await ResourceContext.ResourceGroup.GetPostgreSqlServers()
            .CreateOrUpdateAsync(WaitUntil.Completed, "example-app-test-db", opts);

        var firewallRules = server.Value.GetPostgreSqlFirewallRules();
        await firewallRules.CreateOrUpdateAsync(WaitUntil.Completed, "Azure", new PostgreSqlFirewallRuleData(IPAddress.Any, IPAddress.Any));       
            
        // add my ip
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        var ip = IPAddress.Parse(externalIpString);
        await firewallRules.CreateOrUpdateAsync(WaitUntil.Completed, "Me", new PostgreSqlFirewallRuleData(ip, ip));
        return server.Value.Id;
    }
}