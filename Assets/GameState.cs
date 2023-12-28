using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine.Events;

public class GameState : NetworkBehaviour {
    [SerializeField] TextMeshProUGUI  text;
    private NetworkVariable<int> networkStringTest = new NetworkVariable<int>(0);
    [SerializeField]
    BoardManager boardManager;

    private void Start() {
        boardManager.OnCardAttack += AttemptToPerformAttack;
    }

    private void AttemptToPerformAttack(bool attackerIsPlayer, int attackerIndex, int targetIndex) {
        AttemptToPerformAttackServerRpc(attackerIsPlayer, attackerIndex, targetIndex, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, ServerRpcParams rpcParams) {
        // IReadOnlyList<ulong> clientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        // List<ulong> clientsIdsWithoutCurrentClientId = new List<ulong>();
        // for (int i = 0; i < clientsIds.Count; i++)
        // {
        //     if (clientsIds[i] != rpcParams.Receive.SenderClientId)
        //         clientsIdsWithoutCurrentClientId.Add(clientsIds[i]);
        // }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
            }
        };

        PerformAttackClientRpc(attackerIsPlayer, attackerIndex, targetIndex, clientRpcParams);
    }

    [ClientRpc]
    private void PerformAttackClientRpc(bool attackerIsPlayer, int attackerIndex, int targetIndex, ClientRpcParams rpdParams) {
        
        print("ClientRPCAttack");
        boardManager.PerformAttackByIndex(attackerIsPlayer, attackerIndex, targetIndex);
    }
}
