using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
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
    [SerializeField]
    NetworkControlScheme networkControl;
    [SerializeField]
    BoardManager boardManager;

    private PlayerPair playerPair;
    private Action OnOnePlayerConnected;
    private Action<bool, string> OnPairComplete;

    public void Initialize(Action<bool, string> OnPairComplete, Action OnOnePlayerConnected)
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
            print("clientId = " + clientId);
            ulong opponentID = playerPair.GetOpponentID(clientId);
            print("opponentID = " + opponentID);
            bool isPlayer1 = IsClientIdPlayer(opponentID);
            GameInstance instance = gameInstanceManager.Create(playerPair, networkControl.ForceEndTurn, networkControl.SendTimerStartMessage, OnGameEnd);
            ulong instanceId = Convert.ToUInt64(gameInstanceManager.GetInstanceID(instance));
            SendClientInfo(IsClientIdPlayer(clientId), instance.GameState.GetReveresed().ToJson(), isPairCompleted, instanceId, clientId);
            SendClientInfo(IsClientIdPlayer(playerPair.GetOpponentID(clientId)), instance.GameState.ToJson(), isPairCompleted, instanceId, playerPair.GetOpponentID(clientId));
            playerPair = new PlayerPair(0, 0);
        }
        else
        {
            SendClientInfo(IsClientIdPlayer(clientId), String.Empty, isPairCompleted, 9999, clientId);
        }
    }

    private void OnGameEnd() {
        StartCoroutine(ShutDown());
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
            SendClientShutDownClientRpc(true, clientRpcParams);
        }
        StartCoroutine(ShutDown());
    }

    
    public IEnumerator ShutDown()
    {
        yield return new WaitForSeconds(10);

        Application.Quit();
    }

    private void SendClientInfo(bool isPlayer, string gameStateJson, bool isPairCompleted, ulong instanceId, ulong clientId) {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        SendInfoClientRpc(isPlayer, gameStateJson, isPairCompleted, instanceId, clientRpcParams);
    }

    [ClientRpc]
    private void SendInfoClientRpc(bool isPlayer, string gameStateJson, bool isPairCompleted, ulong instanceId, ClientRpcParams rpdParams) {
        if (isPairCompleted)
            OnPairComplete(isPlayer, gameStateJson);
        else
            OnOnePlayerConnected();
        //instanceIdtext.SetText(instanceId.ToString());
    }
    
    [ClientRpc]
    public void SendClientShutDownClientRpc(bool isVictory, ClientRpcParams rpdParams) {
        boardManager.OnPreGameEnd();
        if (isVictory)
            boardManager.enemyHero.DealDamage(30);
        else 
            boardManager.playerHero.DealDamage(30);


        print("Shutdown from client RPC");
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
    
    public ulong GetIdByPlayerState(PlayerState state)
    {
        if (state == PlayerState.Player) {
            return PlayerID;
        } 
        else
        {
            return EnemyID;
        }
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
