using NahidaImpact.Data;
using NahidaImpact.Data.Excel;
using NahidaImpact.Database;
using NahidaImpact.Database.Avatar;
using NahidaImpact.GameServer.Game.Entity;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.GameServer.Game.Player.Team;
using NahidaImpact.GameServer.Server.Packet.Send.Avatar;
using System.Linq;

namespace NahidaImpact.GameServer.Game.Avatar;

public class AvatarManager(PlayerInstance player) : BasePlayerManager(player)
{
    public AvatarData AvatarData { get; } = DatabaseHelper.GetInstanceOrCreateNew<AvatarData>(player.Uid);
    public List<GameAvatarTeam> AvatarTeams { get; } = new();
    public uint CurTeamIndex { get; private set; }
    public async ValueTask<AvatarDataExcel?> AddAvatar(int avatarId, int level = 90)
    {
        if (AvatarData.Avatars.Any(a => a.AvatarId == avatarId)) return null;
        GameData.AvatarData.TryGetValue(avatarId, out var avatarExcel);
        if (avatarExcel == null) return null;

        uint currentTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        var avatar = new AvatarDataInfo
        {
            Level = (uint)level,
            SkillDepotId = avatarExcel.SkillDepotId,
            AvatarId = avatarExcel.Id,
            Guid = NextGuid(),
            WeaponId = avatarExcel.InitialWeapon,
            BornTime = currentTimestamp,
            WearingFlycloakId = 340005
        };
        
        if (AvatarTeams.Count == 0)
        {
            AvatarTeams.Add(new GameAvatarTeam
            {
                AvatarGuidList = new() { avatar.Guid },
                Index = 1
            });
            CurTeamIndex = 1;
        }
        else
        {
            AvatarTeams[0].AvatarGuidList.Add(avatar.Guid);
        }
        
        avatar.InitDefaultProps(avatarExcel);
        AvatarData.Avatars.Add(avatar);

        return avatarExcel;
    }
    
    public GameAvatarTeam GetCurrentTeam()
    {
        var team = AvatarTeams.FirstOrDefault(team => team.Index == CurTeamIndex);
        if (team != null) return team;

        if (AvatarTeams.Count == 0)
        {
            var fallback = new GameAvatarTeam { Index = 1 };
            AvatarTeams.Add(fallback);
            CurTeamIndex = fallback.Index;
            return fallback;
        }

        CurTeamIndex = AvatarTeams[0].Index;
        return AvatarTeams[0];
    }
    
    public EntityAvatar CreateAvatar(PlayerInstance player, AvatarDataInfo avatarInfo)
    {
        return new(player, avatarInfo, ++player.EntityIdSeed);
    }

    public ulong NextGuid()
    {
        return ((ulong)Player.Uid << 32) + (++Player.GuidSeed);
    }

    public AvatarDataInfo? GetAvatar(int avatarId)
    {
        return AvatarData.Avatars.Find(avatar => avatar.AvatarId == avatarId);
    }
}