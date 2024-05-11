using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System;

public class PlayerConnectionManager : NetworkBehaviour 
{
    private PlayerPair playerPair;
    private Action OnOnePlayerConnected;
    private Action<bool> OnPairComplete;

    public void Initialize(Action<bool> OnPairComplete, Action OnOnePlayerConnected)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        playerPair = new PlayerPair(0, 0);
        this.OnPairComplete = OnPairComplete;
        this.OnOnePlayerConnected = OnOnePlayerConnected;
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
        

        bool isPairCompleted = playerPair.IsPairCompleted();
        SendClientInfo(IsClientIdPlayer(clientId), isPairCompleted, clientId);
        if (isPairCompleted) 
            SendClientInfo(IsClientIdPlayer(clientId), isPairCompleted, playerPair.GetOpponentID(clientId));
    }

    private void SendClientInfo(bool isPlayer, bool isPairCompleted, ulong clientId) {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        SendInfoClientRpc(isPlayer, isPairCompleted, clientRpcParams);
    }

    [ClientRpc]
    private void SendInfoClientRpc(bool isPlayer, bool isPairCompleted, ClientRpcParams rpdParams) {
        print("IsPlayer = " + isPlayer);
        print("isPairCompleted = " + isPairCompleted);

        if (isPairCompleted)
            OnPairComplete(isPlayer);
        else
            OnOnePlayerConnected();
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

    public ulong GetOpponentID(ulong clientId)
    {
        Assert.IsTrue(IsClientIdInPair(clientId));
        if (clientId != PlayerID)
            return PlayerID;
        else
            return EnemyID;
    }

    public bool IsPairCompleted()
    {
        return PlayerID != 0 && EnemyID != 0;
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
