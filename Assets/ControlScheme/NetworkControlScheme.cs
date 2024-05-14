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

    void ControlScheme.AttemptToPerformAttack(bool attackerIsPlayer, int attackerIndex, int targetIndex) {
        AttemptToPerformAttackServerRpc(attackerIsPlayer, attackerIndex, targetIndex, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, ServerRpcParams rpcParams) {
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
        PerformAttackClientRpc(attackerIsPlayer, attackerIndex, targetIndex, playerRpcParams);
        PerformAttackClientRpc(!attackerIsPlayer, attackerIndex, targetIndex, enemyRpcParams);
    }

    [ClientRpc]
    private void PerformAttackClientRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, ClientRpcParams rpdParams) {
        
        print("ClientRPCAttack");
        boardManager.PerformAttackByIndex(attackerIsPlayer, attackerIndex, targetIndex);
    }
}
