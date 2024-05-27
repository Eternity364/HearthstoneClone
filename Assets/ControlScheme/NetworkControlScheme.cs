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
        AttemptToPerformAttackServerRpc(true, attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
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
        
        GameStateInstance.Instance.Attack(attackerState, attackerIndex, targetState, targetIndex);
        string serverHash = GameStateInstance.Instance.GetStringHash();
        string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
        if (serverHash == clientHash) {        
            PerformAttackerMoveClientRpc(playerRpcParams);
            PerformTargetMoveClientRpc(attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), enemyRpcParams);
        }

        print("atta = " + attackerIndex);
        print("tar = " + targetIndex);
    }

    [ClientRpc]
    private void PerformAttackerMoveClientRpc(ClientRpcParams rpdParams) {
        boardManager.PerformAttackByCard(attacker, target);
    }

    [ClientRpc]
    private void PerformTargetMoveClientRpc(int attackerIndex, int targetIndex, byte[] stateHash, ClientRpcParams rpdParams) {
        attacker = boardManager.EnemyCardsOnBoard[attackerIndex];
        target = boardManager.PlayerCardsOnBoard[targetIndex];
        print("atta = " + attackerIndex);
        print("tar = " + targetIndex);
        print(GameStateInstance.Instance.ToJson());
        GameStateInstance.Instance.Attack(PlayerState.Enemy, attackerIndex, PlayerState.Player, targetIndex);
        
        //if (stateHash == GameStateInstance.Instance.GetHash())
            boardManager.PerformAttackByCard(attacker, target);
    }
}
