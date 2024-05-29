using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkControlScheme : NetworkBehaviour, ControlScheme {
    private NetworkVariable<int> networkStringTest = new NetworkVariable<int>(0);
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    PlayerConnectionManager playerConnectionManager;
    [SerializeField]
    ActiveCardController activeCardController;
    [SerializeField]
    Hand opponentHand;
    [SerializeField]
    GameInstanceManager gameInstanceManager;
    [SerializeField]
    Button EndTurnButton;
    [SerializeField]
    EndTurnTimer endTurnTimer;

    private Card attacker, target;
    private Queue<InputBlock> blocks = new Queue<InputBlock>();

    BoardManager ControlScheme.bManager
    {
        get
        {
            return boardManager;
        }
    }
    ActiveCardController ControlScheme.ActiveCardController
    {
        get
        {
            return activeCardController;
        }
    }

    public void AddInputBlock(InputBlock block) {
        blocks.Enqueue(block);
    }

    public void AddInputBlock() {
        InputBlock block = InputBlockerInstace.Instance.AddBlock();
        blocks.Enqueue(block);
    }

    public void DequeueInputBlock() {
        InputBlock block = blocks.Dequeue();
        if (block != null) {
            InputBlockerInstace.Instance.RemoveBlock(block);
        }
    }

    void ControlScheme.AttemptToPerformAttack(PlayerState attackerState, int attackerIndex, int targetIndex) {
        attacker = boardManager.PlayerCardsOnBoard[attackerIndex];
        target = boardManager.EnemyCardsOnBoard[targetIndex];
        boardManager.PlayerCardsOnBoard[attackerIndex].cardDisplay.SetActiveStatus(false);
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
        AttemptToPerformAttackServerRpc(attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        AddInputBlock();
    }

    void ControlScheme.AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
        boardManager.PlayerCardsOnBoard[boardIndex].cardDisplay.SetActiveStatus(true);
        AttemptToPerformCardPlacementServerRpc(handIndex, boardIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        AddInputBlock();
    }

    void ControlScheme.AttemptToStartNextTurn() {
        AddInputBlock();
        EndTurnButton.gameObject.SetActive(false);
        endTurnTimer.gameObject.SetActive(false);
        boardManager.SetCardsStatusActive(false);
        GameStateInstance.Instance.SetCardsActive(PlayerState.Enemy);
        AttemptToStartNextTurnServerRpc(GameStateInstance.Instance.GetHash(), new ServerRpcParams());
    }

    public void SendTimerStartMessage(GameInstance instance) {
        ulong playerId = instance.Pair.PlayerID;
        ulong opponentId = instance.Pair.EnemyID;
        
        ClientRpcParams playerRpcParams = CreateClientRpcParams(playerId);
        ClientRpcParams opponentRpcParams = CreateClientRpcParams(opponentId);
        
        TimerStartClientRpc(playerRpcParams);
        TimerStartClientRpc(opponentRpcParams);
    }
        
    
    public void ForceEndTurn(GameInstance instance) {
        PlayerState currentTurn = instance.currentTurn;
        ulong currentId = instance.Pair.GetIdByPlayerState(currentTurn);
        ulong nextId = instance.Pair.GetOpponentID(currentId);
        instance.currentTimer = 0;
        instance.currentTurn = PlayerState.Enemy;
        if (currentTurn == PlayerState.Enemy)
            instance.currentTurn = PlayerState.Player;

        
        ClientRpcParams currentRpcParams = CreateClientRpcParams(currentId);
        ClientRpcParams nextRpcParams = CreateClientRpcParams(nextId);
        PlayerState nextTurn = PlayerState.Player;
        if (currentTurn == PlayerState.Player) {
            nextTurn = PlayerState.Enemy;
        }

        instance.SetTurn(nextTurn);

        GameState state = instance.GameState;
        if (currentTurn == PlayerState.Player) {
            state = state.GetReveresed();
        }

        SetNewTurnClientRpc(state.GetHash(), true, nextRpcParams);
        SetNewTurnClientRpc(state.GetReveresed().GetHash(), false, currentRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToStartNextTurnServerRpc(byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ulong opponentID = instance.Pair.GetOpponentID(rpcParams.Receive.SenderClientId);
            PlayerState state = PlayerState.Enemy;
            if (instance.Pair.PlayerID == opponentID) {
                state = PlayerState.Player;
            }
            instance.SetTurn(state);
            GameState gameState = instance.GameState;
            if (instance.Pair.PlayerID == opponentID) {
                gameState = instance.GameState.GetReveresed();
            }
            
            ClientRpcParams playerRpcParams = CreateClientRpcParams(opponentID);
            string serverHash = gameState.GetStringHash();
            string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);

            
            if (serverHash == clientHash) { 
                SetNewTurnClientRpc(gameState.GetReveresed().GetHash(), true, playerRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(int attackerIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            bool attackerIsPlayer = true;
            if (!instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId))
                attackerIsPlayer = false;

            PlayerState attackerState = PlayerState.Player;
            PlayerState targetState = PlayerState.Enemy;
            if (!attackerIsPlayer)
            {
                attackerState = PlayerState.Enemy;
                targetState = PlayerState.Player;
            }
            
            instance.GameState.Attack(attackerState, attackerIndex, targetState, targetIndex);
            ClientRpcParams attackerRpcParams = playerRpcParams;
            ClientRpcParams targetRpcParams = enemyRpcParams;
            GameState state = instance.GameState;
            if (!attackerIsPlayer) {
                state = instance.GameState.GetReveresed();
                attackerRpcParams = enemyRpcParams;
                targetRpcParams = playerRpcParams;
            }
            string serverHash = state.GetStringHash();
            string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
            if (serverHash == clientHash) {        
                PerformAttackerMoveClientRpc(attackerRpcParams);
                PerformTargetMoveClientRpc(attackerIndex, targetIndex, state.GetReveresed().GetHash(), targetRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformCardPlacementServerRpc(int handIndex, int boardIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            bool IsPlayer = instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId);

            PlayerState state = PlayerState.Player;
            if (!IsPlayer)
            {
                state = PlayerState.Enemy;
            }
            
            instance.GameState.PlaceCard(state, handIndex, boardIndex);
            ClientRpcParams placementClientRpcParams = enemyRpcParams;
            ClientRpcParams controlReleaseClientRpcParams = playerRpcParams;
            GameState gameState = instance.GameState;
            if (!IsPlayer) {
                gameState = instance.GameState.GetReveresed();
                placementClientRpcParams = playerRpcParams;
                controlReleaseClientRpcParams = enemyRpcParams;
            }
            string serverHash = gameState.GetStringHash();
            string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
            if (serverHash == clientHash) {        
                PerformCardPlacementClientRpc(handIndex, boardIndex, gameState.GetReveresed().GetHash(), placementClientRpcParams);
                PerformControlReleaseClientRpc(controlReleaseClientRpcParams);
            }
        }
    }

    [ClientRpc]
    private void SetNewTurnClientRpc(byte[] stateHash, bool value, ClientRpcParams rpdParams) {
        PlayerState turn = PlayerState.Player;
        if (!value)
            turn = PlayerState.Enemy;
        GameStateInstance.Instance.SetCardsActive(turn);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash) {
            if (value) {
                DequeueInputBlock();
                boardManager.OnPlayerTurnStart();
                
            }
            else {
                AddInputBlock();
                boardManager.SetCardsStatusActive(false);
            }
            EndTurnButton.gameObject.SetActive(value);
            endTurnTimer.gameObject.SetActive(false);
        }
    }
        
    [ClientRpc]
    private void PerformCardPlacementClientRpc(int handIndex, int boardIndex, byte[] stateHash, ClientRpcParams rpdParams) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Enemy, handIndex, boardIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash)
            opponentHand.PlaceCard(opponentHand.cards[handIndex], boardIndex);
    }
        
    [ClientRpc]
    private void PerformControlReleaseClientRpc(ClientRpcParams rpdParams) {
        DequeueInputBlock();
    }

    [ClientRpc]
    private void PerformAttackerMoveClientRpc(ClientRpcParams rpdParams) {
        boardManager.PerformAttackByCard(attacker, target);
        DequeueInputBlock();
    }

    [ClientRpc]
    private void PerformTargetMoveClientRpc(int attackerIndex, int targetIndex, byte[] stateHash, ClientRpcParams rpdParams) {
        attacker = boardManager.EnemyCardsOnBoard[attackerIndex];
        target = boardManager.PlayerCardsOnBoard[targetIndex];
        GameStateInstance.Instance.Attack(PlayerState.Enemy, attackerIndex, PlayerState.Player, targetIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash)
            boardManager.PerformAttackByCard(attacker, target);
    }

    [ClientRpc]
    private void TimerStartClientRpc(ClientRpcParams rpdParams) {
        endTurnTimer.Begin();
        endTurnTimer.gameObject.SetActive(true);
    }

    private ClientRpcParams CreateClientRpcParams(ulong clientId) {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
    }
}
