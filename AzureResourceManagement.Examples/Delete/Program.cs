using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;

namespace AzureResourceManagement.Examples.Delete
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
               await serviceScope.ServiceProvider.GetRequiredService<Executor>().Execute();
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

            services
                .AddSingleton<Executor>()
                .AddSingleton<ArmClient>(_ => new ArmClient(new AzureCliCredential()));

            _serviceProvider = services.BuildServiceProvider();
        }
        
        private static void DisposeServices() => _serviceProvider?.Dispose();
    }
}