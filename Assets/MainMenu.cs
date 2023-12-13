using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject game;
    [SerializeField] private Button startClient;
    [SerializeField] private Button startServer;
    [SerializeField] private GameObject serverStartedText;

    private void Start() {
        startClient.onClick.AddListener(() => {
            StartClient();
        });
        startServer.onClick.AddListener(() => {
            StartServer();
        });
    }

    private void StartClient()
    {
        game.SetActive(true);
        gameObject.SetActive(false);
        NetworkManager.Singleton.StartClient();
    }

    private void StartServer()
    {
        serverStartedText.SetActive(true);
        startClient.gameObject.SetActive(false);
        startServer.gameObject.SetActive(false);
        NetworkManager.Singleton.StartServer();
    }
}
