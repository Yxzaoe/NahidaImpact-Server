using NahidaImpact.Database.Account;
using NahidaImpact.Enums.Player;
using NahidaImpact.GameServer.Server;
using NahidaImpact.Internationalization;
using NahidaImpact.KcpSharp;
using NahidaImpact.Util;
using System.Reflection;
using System.Threading.Tasks;

namespace NahidaImpact.GameServer.Command;

public class CommandManager
{
    public static Logger Logger { get; } = new("CommandManager");

    public static Dictionary<string, ICommands> Commands { get; } = [];
    public static Dictionary<string, CommandInfoAttribute> CommandInfo { get; } = [];
    public static Dictionary<string, string> CommandAlias { get; } = []; // <aliasName, fullName>

    public static void RegisterCommands()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            if (typeof(ICommands).IsAssignableFrom(type) && !type.IsAbstract)
                RegisterCommand(type);

        Logger.Info(I18NManager.Translate("Server.ServerInfo.RegisterItem", Commands.Count.ToString(),
            I18NManager.Translate("Word.Command")));
    }

    public static void RegisterCommand(Type type)
    {
        var attr = type.GetCustomAttribute<CommandInfoAttribute>();
        if (attr == null) return;
        var instance = Activator.CreateInstance(type);
        if (instance is not ICommands command) return;
        Commands.Add(attr.Name, command);
        CommandInfo.Add(attr.Name, attr);

        foreach (var alias in attr.Alias)
        {
            if (!CommandAlias.TryAdd(alias, attr.Name))
                CommandAlias[alias] = attr.Name;
        }
    }

    public static Task HandleCommand(string input, ICommandSender sender)
    {
        return HandleCommandInternalAsync(input, sender);
    }

    private static async Task HandleCommandInternalAsync(string input, ICommandSender sender)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                await sender.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
                return;
            }

            var argInfo = new CommandArg(input, sender);
            if (argInfo.Args.Count == 0)
            {
                await sender.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
                return;
            }

            var target = sender.GetSender();

            foreach (var arg in argInfo.Args.ToList())
            {
                if (string.IsNullOrEmpty(arg) || arg.Length < 2) continue;
                switch (arg[0])
                {
                    case '-':
                        argInfo.Attributes.Add(arg[1..]);
                        break;
                    case '@':
                        _ = int.TryParse(arg[1..], out target);
                        argInfo.Args.Remove(arg);
                        break;
                }
            }
            argInfo.TargetUid = target;
            if (KcpListener.Connections.Values.ToList().Find(item =>
                    (item as Connection)?.Player?.Uid == target) is Connection con)
                argInfo.Target = con;

            var cmdName = argInfo.Args[0];
            if (CommandAlias.TryGetValue(cmdName, out var fullName)) cmdName = fullName;
            if (!Commands.TryGetValue(cmdName, out var command))
            {
                await sender.SendMsg(I18NManager.Translate("Game.Command.Notice.CommandNotFound"));
                return;
            }
            argInfo.Args.RemoveAt(0);
            var cmdInfo = CommandInfo[cmdName];

            if (!AccountData.HasPerm(cmdInfo.Perm, sender.GetSender()))
            {
                await sender.SendMsg(I18NManager.Translate("Game.Command.Notice.NoPermission"));
                return;
            }
            if (argInfo.Target?.Player?.Uid != sender.GetSender() &&
                !AccountData.HasPerm([PermEnum.Other], sender.GetSender()))
            {
                await sender.SendMsg(I18NManager.Translate("Game.Command.Notice.NoPermission"));
                return;
            }

            if (await TryInvokeCommandMethod(command, argInfo)) return;
            if (await TryInvokeDefaultMethod(command, argInfo)) return;

            await sender.SendMsg(I18NManager.Translate(cmdInfo.Usage));
        }
        catch (Exception ex)
        {
            Logger.Error(I18NManager.Translate("Game.Command.Notice.InternalError", ex.ToString()));
        }
    }

    private static async Task<bool> TryInvokeCommandMethod(ICommands command, CommandArg argInfo)
    {
        foreach (var methodInfo in command.GetType().GetMethods())
        {
            var attr = methodInfo.GetCustomAttribute<CommandMethodAttribute>();
            if (attr == null || argInfo.Args.Count == 0 || attr.MethodName != argInfo.Args[0]) continue;

            argInfo.Args.RemoveAt(0);
            await InvokeCommandAsync(command, methodInfo, argInfo);
            return true;
        }

        return false;
    }

    private static async Task<bool> TryInvokeDefaultMethod(ICommands command, CommandArg argInfo)
    {
        foreach (var methodInfo in command.GetType().GetMethods())
        {
            var attr = methodInfo.GetCustomAttribute<CommandDefaultAttribute>();
            if (attr == null) continue;

            await InvokeCommandAsync(command, methodInfo, argInfo);
            return true;
        }

        return false;
    }

    private static async Task InvokeCommandAsync(ICommands command, MethodInfo methodInfo, CommandArg argInfo)
    {
        var result = methodInfo.Invoke(command, new object[] { argInfo });
        switch (result)
        {
            case Task task:
                await task.ConfigureAwait(false);
                return;
            case ValueTask valueTask:
                await valueTask.ConfigureAwait(false);
                return;
        }

        var returnType = methodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            dynamic dynamicTask = result!;
            await dynamicTask;
        }
    }
}