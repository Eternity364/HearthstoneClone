using UnityEngine;
using Unity.Netcode;

public class NetworkControlScheme : NetworkBehaviour, ControlScheme {
    private NetworkVariable<int> networkStringTest = new NetworkVariable<int>(0);
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    PlayerConnectionManager playerConnectionManager;

    
    private Card attacker, target;

    BoardManager ControlScheme.bManager
    {
        get
        {
            return boardManager;
        }
    }

    void ControlScheme.AttemptToPerformAttack(PlayerState attackerState, int attackerIndex, int targetIndex) {
        attacker = boardManager.PlayerCardsOnBoard[attackerIndex];
        target = boardManager.EnemyCardsOnBoard[targetIndex];
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
        AttemptToPerformAttackServerRpc(attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(int attackerIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        ClientRpcParams playerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{playerConnectionManager.PlayerID}
            }
        };

        ClientRpcParams enemyRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{playerConnectionManager.EnemyID}
            }
        };
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
            PerformAttackerMoveClientRpc(attackerRpcParams);
            PerformTargetMoveClientRpc(attackerIndex, targetIndex, state.GetReveresed().GetHash(), targetRpcParams);
        }
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
}
