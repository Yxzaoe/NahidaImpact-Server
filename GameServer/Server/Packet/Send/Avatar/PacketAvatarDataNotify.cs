using NahidaImpact.Database.Avatar;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.GameServer.Game.Player.Team;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;

namespace NahidaImpact.GameServer.Server.Packet.Send.Avatar;

public class PacketAvatarDataNotify : BasePacket
{
    public PacketAvatarDataNotify(PlayerInstance player, List<AvatarDataInfo> Avatars): base(CmdIds.AvatarDataNotify)
    {
        var proto = new AvatarDataNotify()
        {
            CurAvatarTeamId = player.AvatarManager!.CurTeamIndex,
            ChooseAvatarGuid = 228,
            AvatarList = { Avatars.Select(avatar => avatar.ToProto()) }
        };
        
        foreach (GameAvatarTeam team in player.AvatarManager.AvatarTeams)
        {
            AvatarTeam avatarTeam = new();
            avatarTeam.AvatarGuidList.AddRange(team.AvatarGuidList);

            proto.AvatarTeamMap.Add(team.Index, avatarTeam);
            if (team.Index > 4)
            {
                //TODO
            }
        }
        
        SetData(proto);
    }
}