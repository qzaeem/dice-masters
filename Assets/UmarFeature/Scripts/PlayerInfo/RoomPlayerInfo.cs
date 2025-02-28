using DiceGame.ScriptableObjects;
using Fusion;

public class RoomPlayerInfo : NetworkBehaviour
{
    public static RoomPlayerInfo localPlayer;
    public PlayerInfoVariable playerNameData;

    [Networked, OnChangedRender(nameof(PlayerNameChanged))]
    public string playerName { get; set; }

    //  Shared Dictionary for Player Names in the Session
    [Networked] private NetworkDictionary<PlayerRef, string> playerNames { get; }


    private void PlayerNameChanged()
    {

    }
    //public override void Spawned()
    //{
    //    if (Object.HasInputAuthority)
    //    {
    //        localPlayer = this;
    //        RPC_SetPlayerName(Object.InputAuthority, playerNameData.value.playerName);
    //    }
    //}

    //[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    //private void RPC_SetPlayerName(PlayerRef playerRef, string name)
    //{
    //    if (!playerNames.ContainsKey(playerRef))
    //    {
    //        playerNames.Set(playerRef, name);
    //        PrivateRoomHandler.Instance.UpdatePlayerList(playerNames);
    //    }
    //}

    //public override void Render()
    //{
    //    PrivateRoomHandler.Instance.UpdatePlayerList(playerNames);
    //}
}
