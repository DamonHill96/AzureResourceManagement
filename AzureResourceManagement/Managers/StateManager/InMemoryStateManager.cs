using Azure.Core;

namespace AzureResourceManagement.Managers.StateManager;

internal class InMemoryStateManager : IStateManager
{
    private readonly Dictionary<Type, ResourceIdentifier> CreatedResourceIdentifiers = new();

    private readonly Dictionary<string, string> Outputs = new();

    public void Add(Type type, ResourceIdentifier resourceIdentifier) => CreatedResourceIdentifiers.Add(type, resourceIdentifier);

    public Dictionary<Type, ResourceIdentifier> GetResourceIdentifiersForDependencies(AzureResourceDefinition resourceDefinition) =>
        CreatedResourceIdentifiers
            .Where(def => resourceDefinition.DependsOn.Contains(def.Key))
            .ToDictionary(x => x.Key, x => x.Value);

    public Dictionary<string, ResourceIdentifier> GetCreated() =>
        CreatedResourceIdentifiers.ToDictionary(key => key.Key.Name, value => value.Value);

    /// <summary>
    /// Add an output, these can be used to share data across multiple definitions.
    /// Note: Make sure to add the definition generating the output to the DependsOn array in the definition that will use it.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddOutput(string key, string value)
    {
        if (!Outputs.TryAdd(key, value))
        {
            Console.WriteLine($"Could not add {key} as an output because an output with this key already exists");
        }
    }

    public Dictionary<string, string> GetOutputs()
    {
        return Outputs;
    }

    /// <summary>
    /// Gets an output value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueIfNotFound"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string GetOutput(string key, string? valueIfNotFound = null)
    {
        if (Outputs.TryGetValue(key, out var output))
        {
            return output;
        }

        if (valueIfNotFound is not null)
        {
            return valueIfNotFound;
        }

        throw new Exception($"Output {key} was not found, and no default was provided");
    }
}