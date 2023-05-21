namespace AzureResourceManagement.Extensions;

public static class TypeExtensions
{
    public static bool IsAzureResourceDefinition(this Type type)
    {
        return type.IsAssignableTo(typeof(AzureResourceDefinition));
    }
}