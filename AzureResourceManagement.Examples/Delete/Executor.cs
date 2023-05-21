using Azure;
using Azure.ResourceManager;
using static AzureResourceManagement.Examples.Delete.Constants;

namespace AzureResourceManagement.Examples.Delete;

public class Executor
{
    private readonly ArmClient _armClient;

    public Executor(ArmClient armClient)
    {
        _armClient = armClient;
    }

    public async Task Execute()
    {
        var subscription = await _armClient.GetDefaultSubscriptionAsync();

        var rgCollection = subscription.GetResourceGroups();

        if (await rgCollection.ExistsAsync(ResourceGroupName))
        {
            Console.WriteLine($"Found resource group with name {ResourceGroupName}, deleting...");
            var resourceGroup = await rgCollection.GetAsync(ResourceGroupName);
            var deleteResult = await resourceGroup.Value.DeleteAsync(WaitUntil.Completed);

            if (deleteResult.HasCompleted)
            {
                Console.WriteLine($"{ResourceGroupName} deleted successfully.");
            }

            return;
        }

        Console.WriteLine($"{ResourceGroupName} does not exist. It has most likely already been deleted.");
    }
}