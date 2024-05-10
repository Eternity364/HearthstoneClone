using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class PlayerConnectionManager : NetworkBehaviour 
{
    private PlayerPair playerPair;

    public void Initialize()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        playerPair = new PlayerPair(0, 0);
    }

    public bool IsClientIdPlayer(ulong clientId)
    {
        return playerPair.IsClientIdPlayer(clientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (playerPair.PlayerID == 0)
            playerPair.PlayerID = clientId;
        else
            playerPair.EnemyID = clientId;
        

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        SendClientInfoClientRpc(IsClientIdPlayer(clientId), clientRpcParams);
    }

    [ClientRpc]
    private void SendClientInfoClientRpc(bool isPlayer, ClientRpcParams rpdParams) {

        print("IsPlayer = " + isPlayer);
    }
}

public class PlayerPair {
    private ulong playerID;
    private ulong enemyID;


    // zero value means player is not set
    public PlayerPair(ulong playerID, ulong enemyID) {
        PlayerID = playerID;
        EnemyID = enemyID;
    }
    
    public bool IsClientIdInPair(ulong clientId)
    {
        return clientId == PlayerID || clientId == EnemyID;
    }

    public bool IsClientIdPlayer(ulong clientId)
    {
        Assert.IsTrue(IsClientIdInPair(clientId));
        return clientId == PlayerID;
    }

    public ulong PlayerID
    {
        get => playerID;
        set => playerID = value;
    }

    public ulong EnemyID
    {
        get => enemyID;
        set => enemyID = value;
    }
}
