using Azure.ResourceManager.Resources;

namespace AzureResourceManagement.Contexts;

public class VariablesContext
{
    public VariablesContext(SubscriptionData subscriptionData, ResourceGroupData resourceGroupData)
    {
        SubscriptionData = subscriptionData;
        ResourceGroupData = resourceGroupData;
    }

    public SubscriptionData SubscriptionData { get; }
    public ResourceGroupData ResourceGroupData { get; }

    /// <summary>
    /// Load variables from a file
    /// </summary>
    /// <param name="filePath">Full path to the file</param>
    /// <param name="fileContentsResolver">the function to create a Variables object from the file contents</param>
    public Variables LoadFromFile(string filePath, Func<string, Variables> fileContentsResolver)
    {
        var fileContents = File.ReadAllText(filePath);
        return fileContentsResolver(fileContents);
    }
}