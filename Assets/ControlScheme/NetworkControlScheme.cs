using UnityEngine;
using Unity.Netcode;

public class NetworkControlScheme : NetworkBehaviour, ControlScheme {
    private NetworkVariable<int> networkStringTest = new NetworkVariable<int>(0);
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    PlayerConnectionManager playerConnectionManager;

    BoardManager ControlScheme.bManager
    {
        get
        {
            return boardManager;
        }
    }

    void ControlScheme.AttemptToPerformAttack(PlayerState attackerState, int attackerIndex, int targetIndex) {
        GameState gameState = new GameState(boardManager.PlayerCardsOnBoard, boardManager.EnemyCardsOnBoard);
        print(gameState.ToJson());
        gameState.Attack(attackerState, attackerIndex, PlayerState.Enemy, targetIndex);
        AttemptToPerformAttackServerRpc(true, attackerIndex, targetIndex, gameState.GetHash(), new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
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
        if (!playerConnectionManager.IsClientIdPlayer(rpcParams.Receive.SenderClientId))
            attackerIsPlayer = !attackerIsPlayer;

        PlayerState attackerState = PlayerState.Player;
        PlayerState targetState = PlayerState.Enemy;
        if (!attackerIsPlayer)
        {
            attackerState = PlayerState.Enemy;
            targetState = PlayerState.Player;
        }
        
        print(GameStateInstance.Instance.ToJson());
        GameStateInstance.Instance.Attack(attackerState, attackerIndex, targetState, targetIndex);
        string serverHash = GameStateInstance.Instance.GetStringHash();
        string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
        if (serverHash == clientHash) {        
            PerformAttackClientRpc(attackerIsPlayer, attackerIndex, targetIndex, playerRpcParams);
            PerformAttackClientRpc(!attackerIsPlayer, attackerIndex, targetIndex, enemyRpcParams);
        }
    }

    [ClientRpc]
    private void PerformAttackClientRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, ClientRpcParams rpdParams) {
        
        boardManager.PerformAttackByIndex(attackerIsPlayer, attackerIndex, targetIndex);
    }
}
