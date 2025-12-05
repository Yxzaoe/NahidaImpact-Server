namespace NahidaImpact.Proto;

public class CmdIds
{
    public const int None = 0;
    
    public const int GetPlayerTokenReq = 28214;
    public const int GetPlayerTokenRsp = 1574;

    public const int PingReq = 6603;
    public const int PingRsp = 2794;

    // Step 1
    public const int PlayerLoginReq = 24761;
    public const int PlayerDataNotify = 1442;
    public const int AvatarDataNotify = 71;
    public const int OpenStateUpdateNotify = 7518;
    public const int PlayerEnterSceneNotify = 5390;
    public const int PlayerLoginRsp = 1548;

    // Step 2
    public const int EnterSceneReadyReq = 27445;
    public const int EnterScenePeerNotify = 20740;
    public const int EnterSceneReadyRsp = 25384;

    // Step 3
    public const int SceneInitFinishReq = 4530;
    public const int PlayerEnterSceneInfoNotify = 26842;
    public const int SceneTeamUpdateNotify = 25695;
    public const int SceneInitFinishRsp = 26180;

    // Step 4
    public const int EnterSceneDoneReq = 29246;
    public const int SceneEntityAppearNotify = 1050;
    public const int EnterSceneDoneRsp = 9339;

    // Step 5
    public const int PostEnterSceneReq = 26222;
    public const int PostEnterSceneRsp = 23949;

    public const int SetUpAvatarTeamReq = 28806;
    public const int SetUpAvatarTeamRsp = 2990;
    public const int AvatarTeamUpdateNotify = 409;
    public const int DoSetPlayerBornDataNotify = 2616;
    public const int SetPlayerBornDataReq = 1;
    public const int GetPlayerFriendListReq = 27823;
    public const int GetPlayerFriendListRsp = 9180;
}