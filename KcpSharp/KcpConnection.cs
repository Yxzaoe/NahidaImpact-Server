using NahidaImpact.KcpSharp.Base;
using NahidaImpact.Util;
using NahidaImpact.Util.Security;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;

namespace NahidaImpact.KcpSharp;

public class KcpConnection
{
    public const int HANDSHAKE_SIZE = 20;
    public static readonly ConcurrentBag<int> BannedPackets = [];
    private static readonly Logger Logger = new("GameServer");
    public static readonly ConcurrentDictionary<int, string> LogMap = [];

    public static readonly ConcurrentBag<int> IgnoreLog =
    [
    ];

    protected readonly CancellationTokenSource CancelToken;
    protected readonly KcpConversation Conversation;
    public readonly IPEndPoint RemoteEndPoint;

    public string DebugFile = "";
    public bool IsOnline = true;
    public StreamWriter? Writer;

    public KcpConnection(KcpConversation conversation, IPEndPoint remote)
    {
        Conversation = conversation;
        RemoteEndPoint = remote;
        CancelToken = new CancellationTokenSource();
        this.SecretKey = new byte[4096];
        EncryptSeed = Crypto.GenerateEncryptKeyAndSeed(SecretKey);
        Start();
    }
    
    public bool UseSecretKey { get; set; } = false;

    public bool UseDispatchKey { get; set; } = false;
    
    public byte[]? SecretKey { get; set; } = Crypto.ENCRYPT_KEY;
    
    public ulong EncryptSeed { get; private set; } = Crypto.ENCRYPT_SEED;

    public long? ConversationId => Conversation.ConversationId;

    public SessionStateEnum State { get; set; } = SessionStateEnum.INACTIVE;

    public virtual void Start()
    {
        Logger.Info($"New connection from {RemoteEndPoint}.");
        State = SessionStateEnum.WAITING_FOR_TOKEN;
    }

    public virtual void Stop(bool isServerStop = false)
    {
        Conversation.Dispose();
        try
        {
            CancelToken.Cancel();
            CancelToken.Dispose();
        }
        catch
        {
        }

        IsOnline = false;
    }

    public void LogPacket(string sendOrRecv, ushort opcode, byte[] payload)
    {
        if (!ConfigManager.Config.ServerOption.EnableDebug) return;
        try
        {
            //Logger.DebugWriteLine($"{sendOrRecv}: {Enum.GetName(typeof(OpCode), opcode)}({opcode})\r\n{Convert.ToHexString(payload)}");
            if (IgnoreLog.Contains(opcode)) return;
            if (!ConfigManager.Config.ServerOption.DebugDetailMessage) throw new Exception(); // go to catch block
            var typ = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == "NahidaProto")!.GetTypes()
                .First(t => t.Name == $"{LogMap[opcode]}"); //get the type using the packet name
            var descriptor =
                typ.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(
                    null, null) as MessageDescriptor; // get the static property Descriptor
            var packet = descriptor?.Parser.ParseFrom(payload);
            var formatter = JsonFormatter.Default;
            var asJson = formatter.Format(packet);
            var output = $"{sendOrRecv}: {LogMap[opcode]}({opcode})\r\n{asJson}";
            if (ConfigManager.Config.ServerOption.DebugMessage)
                Logger.Debug(output);
            if (DebugFile == "" || !ConfigManager.Config.ServerOption.SavePersonalDebugFile) return;
            var sw = GetWriter();
            sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
            sw.Flush();
        }
        catch
        {
            var output = $"{sendOrRecv}: {LogMap.GetValueOrDefault(opcode, "UnknownPacket")}({opcode})";
            if (ConfigManager.Config.ServerOption.DebugMessage)
                Logger.Debug(output);
            if (DebugFile != "" && ConfigManager.Config.ServerOption.SavePersonalDebugFile)
            {
                var sw = GetWriter();
                sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
                sw.Flush();
            }
        }
    }

    private StreamWriter GetWriter()
    {
        // Create the file if it doesn't exist
        var file = new FileInfo(DebugFile);
        if (!file.Exists)
        {
            Directory.CreateDirectory(file.DirectoryName!);
            File.Create(DebugFile).Dispose();
        }

        Writer ??= new StreamWriter(DebugFile, true);
        return Writer;
    }

    public async Task SendPacket(byte[] packet)
    {
        try
        {
            Crypto.Xor(packet, UseDispatchKey? Crypto.DISPATCH_KEY : SecretKey);
            _ = await Conversation.SendAsync(packet, CancelToken.Token);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SendPacket(BasePacket packet)
    {
        // Test
        if (packet.CmdId <= 0)
        {
            Logger.Debug("Tried to send packet with missing cmd id!");
            return;
        }

        // DO NOT REMOVE (unless we find a way to validate code before sending to client which I don't think we can)
        if (BannedPackets.Contains(packet.CmdId)) return;
        LogPacket("Send", packet.CmdId, packet.Payload);
        // Header
        var packetBytes = packet.BuildPacket();

        try
        {
            await SendPacket(packetBytes);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SendPacket(int cmdId)
    {
        await SendPacket(new BasePacket((ushort)cmdId));
    }
}