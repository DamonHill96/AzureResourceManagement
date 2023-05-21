namespace AzureResourceManagement;

public class Variables : Dictionary<string, string>
{
    public Variables()
    {
    }

    public Variables(Dictionary<string, string> variables)
    {
        foreach (var (key, value) in variables)
        {
            Add(key, value);
        }
    }

    public string GetVariable(string key)
    {
        if (!TryGetValue(key, out var data)) throw new Exception($"Variable {key} has not been defined");
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new Exception($"Variable {key} requires a value");
        }

        return data;
    }
    
    public Variables With(string key, string value)
    {
        Add(key, value);
        return this;
    }
}