using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class GameState : NetworkBehaviour {
    [SerializeField] TextMeshProUGUI  text;
    private NetworkVariable<int> networkStringTest = new NetworkVariable<int>(0);

    private void Update() {
        if (NetworkManager.Singleton.IsConnectedClient && Input.GetKeyDown("k"))
        {
            TestServerRpc(new ServerRpcParams());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestServerRpc(ServerRpcParams rpcParams) {
        networkStringTest.Value = Random.Range(0, 100);

        IReadOnlyList<ulong> clientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        List<ulong> clientsIdsWithoutCurrentClientId = new List<ulong>();
        for (int i = 0; i < clientsIds.Count; i++)
        {
            if (clientsIds[i] != rpcParams.Receive.SenderClientId)
                clientsIdsWithoutCurrentClientId.Add(clientsIds[i]);
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientsIdsWithoutCurrentClientId.AsReadOnly()
            }
        };

        TestClientRpc(networkStringTest.Value, clientRpcParams);
    }

    [ClientRpc]
    private void TestClientRpc(int value, ClientRpcParams clientRpcParams = default) {
        text.text = value.ToString();
    }
}
