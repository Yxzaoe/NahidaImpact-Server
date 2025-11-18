using System.Security.Cryptography;
using NahidaImpact.Data;
using NahidaImpact.Database;
using NahidaImpact.Database.Account;
using NahidaImpact.Database.Player;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.GameServer.Server.Packet.Send.Player;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;
using NahidaImpact.Util;
using NahidaImpact.Util.Security;

namespace NahidaImpact.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.GetPlayerTokenReq)]
public class HandlerGetPlayerTokenReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetPlayerTokenReq.Parser.ParseFrom(data);
        var account = AccountData.GetAccountByUid(int.Parse(req.AccountUid));
        if (account == null) 
        {
            return;
        }
        if (!ResourceManager.IsLoaded)
            // resource manager not loaded, return
            return;
        var prev = Listener.GetActiveConnection(account.Uid);
        if (prev != null)
        {
            prev.Stop();
        }

        connection.State = SessionStateEnum.WAITING_FOR_LOGIN;
        var pd = DatabaseHelper.GetInstance<PlayerData>(int.Parse(req.AccountUid));
        connection.Player = pd == null ? new PlayerInstance(int.Parse(req.AccountUid)) : new PlayerInstance(pd);

        connection.DebugFile = Path.Combine(ConfigManager.Config.Path.LogPath, "Debug/", $"{req.AccountUid}/",
            $"Debug-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.log");

        await connection.Player.OnGetToken();
        connection.Player.Connection = connection;

        // await connection.SendPacket(new PacketGetPlayerTokenRsp(connection, req.AccountToken, req.KeyId)); // TODO 加上判断
        
        connection.SecretKey = Crypto.GenerateSecretKey(1337);
        connection.State = SessionStateEnum.WAITING_FOR_LOGIN;

        // Only Game Version >= 2.7.50 has this
        if (req.KeyId > 0)
        {
            try
            {
                RSA signer = Crypto.SigningKey;
        
                byte[] client_seed_encrypted = Convert.FromBase64String(req.ClientRandKey);
                byte[] client_seed = signer.Decrypt(client_seed_encrypted, RSAEncryptionPadding.Pkcs1);
                byte[] encryptSeed = BitConverter.GetBytes(connection.EncryptSeed);
                Crypto.Xor(client_seed, encryptSeed);
                byte[] seed_bytes = client_seed;
        
                //Kind of a hack, but whatever
                RSA encryptor = Crypto.GetDispatchEncryptionKey((int)req.KeyId);
                byte[] seed_encrypted = encryptor.Encrypt(seed_bytes, RSAEncryptionPadding.Pkcs1);
        
                byte[] seed_bytes_sign = signer.SignData(seed_bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
                await connection.SendPacket(new PacketGetPlayerTokenRsp(connection, connection.EncryptSeed, Convert.ToBase64String(seed_encrypted), Convert.ToBase64String(seed_bytes_sign), req.AccountToken));
                // Set session state
                connection.UseSecretKey = true;
            }
            catch (Exception ignore)
            {
                // Only UA Patch users will have exception
                byte[] clientBytes = Convert.FromBase64String(req.ClientRandKey);
                byte[] seed = BitConverter.GetBytes(connection.EncryptSeed);
                Crypto.Xor(clientBytes, seed);
        
                string base64str = Convert.ToBase64String(clientBytes);
        
                await connection.SendPacket(new PacketGetPlayerTokenRsp(connection, connection.EncryptSeed, base64str, "bm90aGluZyBoZXJl", req.AccountToken));
                // Set session state
                connection.UseSecretKey = true;
            }
        }
        else
        {
            // Send packet
            await connection.SendPacket(new PacketGetPlayerTokenRsp(connection, req.AccountToken));
            // Set session state
            connection.UseSecretKey = true;
        }
    }
}