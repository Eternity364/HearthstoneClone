using UnityEngine;
using Unity.Netcode;

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

    
    private Card attacker, target;

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

    void ControlScheme.AttemptToPerformAttack(PlayerState attackerState, int attackerIndex, int targetIndex) {
        attacker = boardManager.PlayerCardsOnBoard[attackerIndex];
        target = boardManager.EnemyCardsOnBoard[targetIndex];
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
        AttemptToPerformAttackServerRpc(attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
    }

    void ControlScheme.AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
        AttemptToPerformCardPlacementServerRpc(handIndex, boardIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(int attackerIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        ClientRpcParams playerRpcParams = CreateClientRpcParams(playerConnectionManager.PlayerID);
        ClientRpcParams enemyRpcParams = CreateClientRpcParams(playerConnectionManager.EnemyID);

        bool attackerIsPlayer = true;
        if (!playerConnectionManager.IsClientIdPlayer(rpcParams.Receive.SenderClientId))
            attackerIsPlayer = false;

        PlayerState attackerState = PlayerState.Player;
        PlayerState targetState = PlayerState.Enemy;
        if (!attackerIsPlayer)
        {
            attackerState = PlayerState.Enemy;
            targetState = PlayerState.Player;
        }
        
        GameStateInstance.Instance.Attack(attackerState, attackerIndex, targetState, targetIndex);
        ClientRpcParams attackerRpcParams = playerRpcParams;
        ClientRpcParams targetRpcParams = enemyRpcParams;
        GameState state = GameStateInstance.Instance;
        if (!attackerIsPlayer) {
            state = GameStateInstance.Instance.GetReveresed();
            attackerRpcParams = enemyRpcParams;
            targetRpcParams = playerRpcParams;
        }
        string serverHash = state.GetStringHash();
        string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
        if (serverHash == clientHash) {        
            state.GetReveresed().PrintCounts();
            PerformAttackerMoveClientRpc(attackerRpcParams);
            PerformTargetMoveClientRpc(attackerIndex, targetIndex, state.GetReveresed().GetHash(), targetRpcParams);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformCardPlacementServerRpc(int handIndex, int boardIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        ClientRpcParams playerRpcParams = CreateClientRpcParams(playerConnectionManager.PlayerID);
        ClientRpcParams enemyRpcParams = CreateClientRpcParams(playerConnectionManager.EnemyID);

        bool IsPlayer = playerConnectionManager.IsClientIdPlayer(rpcParams.Receive.SenderClientId);

        PlayerState state = PlayerState.Player;
        if (!IsPlayer)
        {
            state = PlayerState.Enemy;
        }
        
        GameStateInstance.Instance.PlaceCard(state, handIndex, boardIndex);
        ClientRpcParams placementClientRpcParams = enemyRpcParams;
        ClientRpcParams controlReleaseClientRpcParams = playerRpcParams;
        GameState gameState = GameStateInstance.Instance;
        if (!IsPlayer) {
            gameState = GameStateInstance.Instance.GetReveresed();
            placementClientRpcParams = playerRpcParams;
            controlReleaseClientRpcParams = enemyRpcParams;
        }
        string serverHash = gameState.GetStringHash();
        string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
        if (serverHash == clientHash) {        
            //gameState.GetReveresed().PrintCounts();
            PerformCardPlacementClientRpc(handIndex, boardIndex, gameState.GetReveresed().GetHash(), placementClientRpcParams);
            PerformControlReleaseClientRpc(controlReleaseClientRpcParams);
        }
    }
        
    [ClientRpc]
    private void PerformCardPlacementClientRpc(int handIndex, int boardIndex, byte[] stateHash, ClientRpcParams rpdParams) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Enemy, handIndex, boardIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        //GameStateInstance.Instance.PrintCounts();
        if (serverHash == clientHash)
            opponentHand.PlaceCard(opponentHand.cards[handIndex], boardIndex);
    }
        
    [ClientRpc]
    private void PerformControlReleaseClientRpc(ClientRpcParams rpdParams) {
        //boardManager.PerformAttackByCard(attacker, target);
    }

    [ClientRpc]
    private void PerformAttackerMoveClientRpc(ClientRpcParams rpdParams) {
        boardManager.PerformAttackByCard(attacker, target);
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
