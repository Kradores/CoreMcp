namespace CoreMcp.Framework.Reflection;

internal static class GenericTypeHelper
{
    public static Type FindClosedGenericInterface(
        Type implementationType,
        Type openGenericInterface)
    {
        ArgumentNullException.ThrowIfNull(implementationType);
        ArgumentNullException.ThrowIfNull(openGenericInterface);

        var matches = implementationType
            .GetInterfaces()
            .Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == openGenericInterface)
            .ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"{implementationType.Name} does not implement {openGenericInterface.Name}.");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"{implementationType.Name} implements {openGenericInterface.Name} multiple times.");
        }

        return matches[0];
    }
}
