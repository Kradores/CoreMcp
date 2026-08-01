using CoreMcp.Framework.Handlers;
using CoreMcp.Framework.Reflection;
using CoreMcp.Framework.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMcp.Framework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpHandler<THandler>(
        this IServiceCollection services)
        where THandler : class
    {
        var implementationType = typeof(THandler);

        var handlerInterface =
            GenericTypeHelper.FindClosedGenericInterface(
                implementationType,
                typeof(IMcpMethodHandler<,>));

        services.AddSingleton(
            handlerInterface,
            implementationType);

        var genericArguments =
            handlerInterface.GetGenericArguments();

        var adapterType =
            typeof(HandlerAdapter<,>)
                .MakeGenericType(genericArguments);

        services.AddSingleton(
            typeof(IHandlerAdapter),
            sp =>
            {
                var handler =
                    sp.GetRequiredService(handlerInterface);

                return (IHandlerAdapter)
                    ActivatorUtilities.CreateInstance(
                        sp,
                        adapterType,
                        handler);
            });

        return services;
    }

    public static IServiceCollection AddMcpTool<TTool>(
    this IServiceCollection services)
    where TTool : class
    {
        var implementationType = typeof(TTool);

        var toolInterface =
            GenericTypeHelper.FindClosedGenericInterface(
                implementationType,
                typeof(IMcpTool<>));

        services.AddSingleton(
            toolInterface,
            implementationType);

        var argumentsType =
            toolInterface.GetGenericArguments()[0];

        var adapterType =
            typeof(ToolAdapter<>)
                .MakeGenericType(argumentsType);

        services.AddSingleton(
            typeof(IToolAdapter),
            sp =>
            {
                var tool =
                    sp.GetRequiredService(toolInterface);

                return (IToolAdapter)
                    ActivatorUtilities.CreateInstance(
                        sp,
                        adapterType,
                        tool);
            });

        return services;
    }
}
