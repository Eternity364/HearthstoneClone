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
    [SerializeField]
    GameInstanceManager gameInstanceManager;

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
        InputBlockerInstace.Instance.SetActive(false);
    }

    void ControlScheme.AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
        AttemptToPerformCardPlacementServerRpc(handIndex, boardIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        InputBlockerInstace.Instance.SetActive(false);
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
        InputBlockerInstace.Instance.SetActive(true);
    }

    [ClientRpc]
    private void PerformAttackerMoveClientRpc(ClientRpcParams rpdParams) {
        boardManager.PerformAttackByCard(attacker, target);
        InputBlockerInstace.Instance.SetActive(true);
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
