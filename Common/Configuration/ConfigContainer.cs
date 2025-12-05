namespace NahidaImpact.Configuration;

public class ConfigContainer
{
    public HttpServerConfig HttpServer { get; set; } = new();
    public GameServerConfig GameServer { get; set; } = new();
    public PathConfig Path { get; set; } = new();
    public ServerOption ServerOption { get; set; } = new();
    public static ServerProfile ServerProfile { get; set; } = new();
}

public class HttpServerConfig
{
    public string BindAddress { get; set; } = "0.0.0.0";
    public string PublicAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 2712;

    public string GetDisplayAddress()
    {
        return "http" + "://" + PublicAddress + ":" + Port;
    }

    public string GetBindDisplayAddress()
    {
        return "http" + "://" + BindAddress + ":" + Port;
    }
}

public class GameServerConfig
{
    public string BindAddress { get; set; } = "0.0.0.0";
    public string PublicAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 22102;
    public int KcpAliveMs { get; set; } = 30000;
    public string DatabaseName { get; set; } = "Nahida.db";
    public string GameServer { get; set; } = "NahidaImpact";
    public string GameServerName { get; set; } = "NahidaImpact";
    
    public string GetDisplayAddress()
    {
        return PublicAddress + ":" + Port;
    }
}

public class PathConfig
{
    public string ResourcePath { get; set; } = "Resources";
    public string ConfigPath { get; set; } = "Config";
    public string DatabasePath { get; set; } = "Config/Database";
    public string HandbookPath { get; set; } = "Config/Handbook";
    public string LogPath { get; set; } = "Config/Logs";
    public string DataPath { get; set; } = "Config/Data";
}

public class ServerOption
{
    public string Language { get; set; } = "EN";
    public string[] DefaultPermissions { get; set; } = ["Admin"];
    public bool AutoCreateUser { get; set; } = true;
    public bool SavePersonalDebugFile { get; set; } = false;
#if DEBUG
    public bool EnableDebug { get; set; } = true;
#else
    public bool EnableDebug { get; set; } = false;
#endif
    public bool DebugMessage { get; set; } = true;
    public bool DebugDetailMessage { get; set; } = true;
    public bool DebugNoHandlerPacket { get; set; } = true;
    public bool IsServerStop { get; set; } = false;
    public bool UseXorEncryption { get; set; } = true;
    
}

public class ServerProfile
{
    public string NickName { get; set; } = "Nahida";
    public string Signature = "Welcome to NahidaServer!";
    public int Uid { get; set; } = 511694508;
    public int AvatarId { get; set; } = 10000007;
    public int NameCardId = 210001;
    public int Level { get; set; } = 60;
    public int WorldLevel = 9;
}
