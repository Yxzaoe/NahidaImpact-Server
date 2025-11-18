using NahidaImpact.GameServer.Server.Packet.Send.Player;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;

namespace NahidaImpact.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.PlayerLoginReq)]
public class HandlerPlayerLoginReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        connection.State = SessionStateEnum.ACTIVE;
        
        // Check
        if (connection.Player == null)
        {
            connection.Stop();
            return;
        }
        
        await connection.Player.OnLogin();
        await connection.SendPacket(new PacketPlayerLoginRsp(connection));
    }
}
