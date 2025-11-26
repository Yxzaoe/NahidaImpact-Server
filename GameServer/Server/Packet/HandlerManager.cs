using System.Reflection;

namespace NahidaImpact.GameServer.Server.Packet;

public static class HandlerManager
{
    private static readonly Dictionary<int, Handler> Handlers = [];

    public static void Init()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!typeof(Handler).IsAssignableFrom(type) || type.IsAbstract) continue;

            var attribute = type.GetCustomAttribute<Opcode>();
            if (attribute == null) continue;

            var handler = (Handler?)Activator.CreateInstance(type);
            if (handler == null) continue;

            Handlers[attribute.CmdId] = handler;
        }
    }

    public static Handler? GetHandler(int cmdId)
    {
        Handlers.TryGetValue(cmdId, out var handler);
        return handler;
    }
}