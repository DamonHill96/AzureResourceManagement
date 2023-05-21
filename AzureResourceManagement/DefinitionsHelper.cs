namespace AzureResourceManagement;

public static class DefinitionsHelper
{
    public static IEnumerable<Type> GetAzureResourceDefinitions()
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()
            .Where(p => typeof(AzureResourceDefinition).IsAssignableFrom(p) && !p.IsAbstract));
    }
    
    public static IEnumerable<Type> GetAzureResourceDefinitionsFromAssembly<TAssembly>()
    {
        return typeof(TAssembly).Assembly.GetTypes().Where(p => typeof(AzureResourceDefinition).IsAssignableFrom(p) && !p.IsAbstract);
    }
}