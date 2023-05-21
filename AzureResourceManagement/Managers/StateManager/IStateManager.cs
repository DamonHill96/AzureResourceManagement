using Azure.Core;

namespace AzureResourceManagement.Managers.StateManager;

internal interface IStateManager
{
    public void Add(Type type, ResourceIdentifier resourceIdentifier);

    public Dictionary<Type, ResourceIdentifier> GetResourceIdentifiersForDependencies(AzureResourceDefinition resourceDefinition);
    public Dictionary<string, ResourceIdentifier> GetCreated();
    /// <summary>
    /// Add an output, these can be used to share data across multiple definitions.
    /// Note: Make sure to add the definition generating the output to the DependsOn array in the definition that will use it.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddOutput(string key, string value);
    public Dictionary<string, string> GetOutputs();

    /// <summary>
    /// Gets an output value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueIfNotFound"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string GetOutput(string key, string? valueIfNotFound = null);
}