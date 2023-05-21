using Azure.Core;
using AzureResourceManagement.Configuration;
using AzureResourceManagement.Examples.Create.Definitions.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AzureResourceManagement.Examples.Create
{
    class Program
    {
        private static ServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            RegisterServices();
            var serviceScope = _serviceProvider.CreateScope();
            try
            {
                var res = await serviceScope.ServiceProvider.GetRequiredService<IResourceCreator>().ExecuteAsync();
                Console.WriteLine("debug");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
                Environment.Exit(1);
            }

            DisposeServices();
            Environment.Exit(0);
        }

        /// <summary>
        /// Register services into the DI Container
        /// </summary>
        private static void RegisterServices()
        {
            var services = new ServiceCollection();

            //  var config = RegisterConfigurationFile();
            services
                .AddAzureResourceManagement(opts =>
                {
                    opts.ResourceGroupName = Constants.ResourceGroupName;
                    opts.ResourceGroupLocation = AzureLocation.UKSouth;
                    opts.TagsFactory = variables => new Dictionary<string, string>
                    {
                        ["Prefix"] = variables.GetVariable("ResourcePrefix")
                    };
                    opts.VariablesFactory = ctx =>
                    {
                        var variables = ctx.LoadFromFile("Variables.json", JsonConvert.DeserializeObject<Variables>)
                            .With("TenantId", ctx.SubscriptionData.TenantId.ToString()!)
                            .With("ResourceGroupName", ctx.ResourceGroupData.Name)
                            .With("UIFilesPath", @"C:\Projects\Example\ExampleUI\dist\example-ui");

                        return variables;
                    };
                    opts.ResourceOptions.ExcludeAllExcept(typeof(Storage));
                    opts.ResourceOptions.AddOptionsForResource(typeof(Storage), resOpts => resOpts.DontWriteTags());
                });

            _serviceProvider = services.BuildServiceProvider();
        }

        private static IConfigurationRoot RegisterConfigurationFile() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

        private static void DisposeServices() => _serviceProvider?.Dispose();
    }
}