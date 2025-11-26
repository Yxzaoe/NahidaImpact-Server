using Google.Protobuf;
using System;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;
using NahidaImpact.Util;
using NahidaImpact.Util.Security;

namespace NahidaImpact.GameServer.Server.Packet.Send.Player;

public class PacketPlayerLoginRsp : BasePacket
{
    private static readonly Logger Logger = new("PacketPlayerLoginRsp");
    private static QueryCurrRegionHttpRsp? RegionCache;
    
    public PacketPlayerLoginRsp(Connection connection) : base(CmdIds.PlayerLoginRsp)
    {
        if (RegionCache == null)
        {
            try
            {
                RegionInfo serverRegion = new()
                {
                    GateserverIp = ConfigManager.Config.GameServer.PublicAddress,
                    GateserverPort = (uint)ConfigManager.Config.GameServer.Port,
                    SecretKey = ByteString.CopyFrom(Crypto.DISPATCH_SEED),
                    ResVersionConfig = new(),
                };
                RegionCache = new QueryCurrRegionHttpRsp
                {
                    RegionInfo = serverRegion
                };
            }
            catch (Exception e)
            {
                Logger.Error("Error while initializing region cache!", e);
                throw;
            }
        }
        
        var info = RegionCache.RegionInfo ?? throw new InvalidOperationException("Region info is not initialized.");
        
        var proto = new PlayerLoginRsp
        {
            // IsUseAbilityHash = true,
            // AbilityHashCode = 1844674,
            GameBiz = "hk4e_global",
            ClientDataVersion = info.ClientDataVersion,
            ClientSilenceDataVersion = info.ClientSilenceDataVersion,
            // ClientMd5 = info.ClientDataMd5,
            // ClientSilenceMd5 = info.ClientSilenceDataMd5,
            ResVersionConfig = info.ResVersionConfig,
            // ClientVersionSuffix = info.ClientVersionSuffix,
            // ClientSilenceVersionSuffix = info.ClientSilenceVersionSuffix,
            // IsScOpen = false,
            // RegisterCps = "mihoyo",
            // CountryCode = "US"
        };

        SetData(proto);
    }
}
