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
            TestServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestServerRpc() {
        networkStringTest.Value = Random.Range(0, 100);
        text.text = networkStringTest.Value.ToString();
    }
}
