using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System;
using TMPro;

public class PlayerConnectionManager : NetworkBehaviour 
{
    [SerializeField]
    GameInstanceManager gameInstanceManager;
    [SerializeField]
    GameObject game;
    [SerializeField]
    TextMeshProUGUI instanceIdtext;

    private PlayerPair playerPair;
    private Action OnOnePlayerConnected;
    private Action<bool> OnPairComplete;

    public void Initialize(Action<bool> OnPairComplete, Action OnOnePlayerConnected)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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
        bool isPlayer = IsClientIdPlayer(clientId);
        if (isPairCompleted) {
            ulong opponentID = playerPair.GetOpponentID(clientId);
            bool isPlayer1 = IsClientIdPlayer(opponentID);
            GameInstance instance = gameInstanceManager.Create(playerPair);
            ulong instanceId = Convert.ToUInt64(gameInstanceManager.GetInstanceID(instance));
            SendClientInfo(IsClientIdPlayer(clientId), isPairCompleted, instanceId, clientId);
            SendClientInfo(IsClientIdPlayer(playerPair.GetOpponentID(clientId)), isPairCompleted, instanceId, playerPair.GetOpponentID(clientId));
            playerPair = new PlayerPair(0, 0);
        }
        else
        {
            SendClientInfo(IsClientIdPlayer(clientId), isPairCompleted, 9999, clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(clientId);
        if (instance != null) {
            ulong opponentID = instance.Pair.GetOpponentID(clientId);
            gameInstanceManager.Remove(instance);
            
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{opponentID}
                }
            };
            SendClientShutDownClientRpc(clientRpcParams);
        }
    }

    private void SendClientInfo(bool isPlayer, bool isPairCompleted, ulong instanceId, ulong clientId) {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        SendInfoClientRpc(isPlayer, isPairCompleted, instanceId, clientRpcParams);
    }

    [ClientRpc]
    private void SendInfoClientRpc(bool isPlayer, bool isPairCompleted, ulong instanceId, ClientRpcParams rpdParams) {
        if (isPairCompleted)
            OnPairComplete(isPlayer);
        else
            OnOnePlayerConnected();
        instanceIdtext.SetText(instanceId.ToString());
    }
    
    [ClientRpc]
    private void SendClientShutDownClientRpc(ClientRpcParams rpdParams) {
        game.SetActive(false);
        Application.Quit();
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
