using Google.Protobuf;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;
using NahidaImpact.Util;
using NahidaImpact.Util.Security;

namespace NahidaImpact.GameServer.Server.Packet.Send.Player;

public class PacketGetPlayerTokenRsp : BasePacket
{
    public PacketGetPlayerTokenRsp(Connection connection, string token) :base(CmdIds.GetPlayerTokenRsp)
    {
        connection.UseDispatchKey = true;
        
        var proto = new GetPlayerTokenRsp
        {
            Uid = (uint)connection.Player.Uid,
            Token = token,
            SecurityCmdBuffer = ByteString.CopyFrom(Crypto.ENCRYPT_SEED_BUFFER),
            PlatformType = 3,
            ChannelId = 1,
            CountryCode = "US",
            // ClientVersionRandomKey = "c25-314dd05b0b5f",
        };

        SetData(proto);
    }
    
    public PacketGetPlayerTokenRsp(Connection connection,string token, uint keyId) : base(CmdIds.GetPlayerTokenRsp)
    {
        connection.UseDispatchKey = true;
        
        var serverRandKey = "";
        var sign = "";
        if (ConfigManager.Config.ServerOption.UseXorEncryption) {
            if (keyId == 4) {
                // CN Rand Key and Sign
                serverRandKey = "Lsu2ERvY+NHM//Ur94+JVUhf+nRj26mmUcP4/SkfnQq/lIGzwgxwYg38qWSMvwqx/xzrrWCoLPyfHj0UwpiBdN5JuVSlwumObpH0NjwCbBhKU1SOrbw48E7EVvGcDKuZrGGy5xPZFaWMtJ6I26ApjdWwVolqYK+zuF2doBkGD00OlZwl6oQMb+nPY2BDrWVW4Uf35/EscvLNfNk1YFhze1ieUCNbAnVjVD0Z0gVRn0cB69MRDrUa5QHI8gePSDIZxYKW4lNxk25lCtbt4fIkhf5iV0dSYuZukJbKYog5zDDTymNUfJLNmlJktvNW3TNyIirTXtl76f9WOJA7mOhsUw==";
                sign = "kwTHFK/78ZEzlYYwIUwt/fw7CBNc/+a7qI7QVk8rym2wsuUz4pk/LbUnd40G4auMJ4yR0pWv5a2yzPJjspD3T/sTAx8uJZN7g15zXKyT2o2zCZV0MzcBUd3pKDcCAFdX1lkK3tphRXUZV62qatVL+ZYTV5ZB5ecxQrZD9MD+omV867YwLExia91YmtnhSLBZ1tfc2m7dn12/3Bl4EIavednhyHYZcCqagnw8mHjTtMz6NHq0/qXNfq8XtCbkA+Ue09veiijvDo9WL1Vz0dosjer3wv3w89CQ+Q5nBDZuY93GiQ6V50WR+737CaIuXB8HOT2RW6eLhPtUmwPWesX86A==";
            } else if (keyId == 5) {
                // OS Rand Key and Sign
                serverRandKey = "CfO2d7eEYha5bJRXdCfoiemPNAtXDpyNTQ3ObeTt5a7SSHz6GAEO1WPiTQ7fR6OG8LqhVN3ZTxH9Bnkc09BnCxud+kn0+PiGv1PTOuWK0LkQQ1xmg89zA9IHS+OJd1yKT2BBmJf4sN61gi+WtT7aFwRlzku3kGCk6p2wiPo2enE7UwCFi/GiD4vq/m3hNZiKBjitAvheaqbSLjMpBax+c8HXoY5G09ap1PjEnUQPIK0xZRRQKpnrWcCyP4j8N3WwYYQGDW+OYOJjBvJdv+D6XSdEi+4IsZASYVpu9V8UZ570Cakbc+IjUm0UZJXghcR7izIjKtoNHf2Fmc26DEp1Jw==";
                sign = "mMx/Klovbzq1QxQvVgm30nYhj0jDOykyo9aparyWRNz3ACxV/2gIdLpyM/SMerWMTcx26NapQ9HsKK7BRK7Yx+nMR0O83BkBlxfl+NEarYr6kj9lBKAxZYXTXFRYA4sRynvwa/MOPmGwYMNl6aVvMohhvrsTopsRvIuGFtnCVL2wBfbxcNnbVfP5k+DxPuQnxa/vi+ju8TogW2R+r0p9zQ5NJe1oaYe4xYbyhefFVv11FA/JQHwMHLEyrEdPqTzdN75CUmE09yLuAoeJzoJ1vwwjwfcH9dMDPxsewNJBGiylVHYf56kF4HypNkYNjtxbghgLBaHg0ZoeYHTOJ7YUTQ==";
            }
        }
        
        var proto = new GetPlayerTokenRsp
        {
            Uid = (uint)connection.Player.Uid,
            Token = token,
            SecurityCmdBuffer = ByteString.CopyFrom(Crypto.ENCRYPT_SEED_BUFFER),
            KeyId = keyId,
            ServerRandKey = serverRandKey,
            Sign = sign,
            PlatformType = 3,
            ChannelId = 1,
            CountryCode = "US",
            // ClientVersionRandomKey = "c25-314dd05b0b5f",
        };

        SetData(proto);
    }
    
    public PacketGetPlayerTokenRsp(Connection connection, ulong secretKeySeed, string encryptedSeed, string encryptedSeedSign, string token) : base(CmdIds.GetPlayerTokenRsp)
    {
        connection.UseDispatchKey = true;
        
        GetPlayerTokenRsp proto = new GetPlayerTokenRsp()
        {
            Uid = (uint)connection.Player.Uid,
            Token = token,
            SecretKeySeed = secretKeySeed,
            SecurityCmdBuffer = ByteString.CopyFrom(Crypto.ENCRYPT_SEED_BUFFER),
            PlatformType = 3,
            ChannelId = 1,
            CountryCode = "US",
            // ClientVersionRandomKey = "c25-314dd05b0b5f",
            ServerRandKey = encryptedSeed,
            Sign = encryptedSeedSign
        };

        SetData(proto);
    }
}