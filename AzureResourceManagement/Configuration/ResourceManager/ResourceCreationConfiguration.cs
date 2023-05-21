using AzureResourceManagement.Extensions;

namespace AzureResourceManagement.Configuration.ResourceManager;

public class ResourceCreationConfiguration
{
    private readonly Dictionary<Type, ResourceOptions> _dict = new();

    /// <summary>
    /// A list of ResourceDefinitions to create.
    /// Useful for testing when you only want to create a few of your defined resources.
    /// Leaving this empty will create resources from all <see cref="AzureResourceDefinition"/> classes
    /// Except those that are excluded in the ResourceOptions
    /// </summary>
    public void ExcludeAllExcept(params Type[] types)
    {
        var allDefinitionsToExclude = DefinitionsHelper.GetAzureResourceDefinitions()
            .Except(types);

        foreach (var def in allDefinitionsToExclude)
        {
            AddOptionsForResource(def, options => options.Exclude());
        }

        return;
    }

    public ResourceCreationConfiguration AddOptionsForResource(Type type, Action<ResourceOptions> opts)
    {
        if (!type.IsAzureResourceDefinition())
        {
            throw new Exception($"Type {type.Name} does not inherit from {nameof(AzureResourceDefinition)}.");
        }
        var optionsObj = new ResourceOptions();
        opts(optionsObj);

        if (_dict.ContainsKey(type))
        {
            _dict[type] = optionsObj;
        }
        else
        {
            _dict.Add(type, optionsObj);
        }
 
        return this;
    }

    internal ResourceOptions GetOptionsForType(Type type)
    {
        if (_dict.TryGetValue(type, out var options))
        {
            return options;
        }
        
        //default
        return new ResourceOptions();
    }
}

public class ResourceOptions
{
    internal bool DontWriteTagsOnCreate { get; set; }
    internal bool DontCreateThisResource { get; set; }

    /// <summary>
    /// By default, any tags that are resolved from the TagsFactory will be applied to all resources.
    /// Call this if you want ot exclude this resource from having tags applied.
    /// </summary>
    /// <returns></returns>
    public ResourceOptions DontWriteTags()
    {
        DontWriteTagsOnCreate = true;
        return this;
    }  
    
    /// <summary>
    /// This resource, and any that depend on it, will not be created
    /// </summary>
    /// <returns></returns>
    public ResourceOptions Exclude()
    {
        DontCreateThisResource = true;
        return this;
    }
}