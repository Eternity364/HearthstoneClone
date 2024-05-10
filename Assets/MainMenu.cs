using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject game;
    [SerializeField] private Button startClient;
    [SerializeField] private Button startServer;
    [SerializeField] private Button startSinglePlayer;
    [SerializeField] private GameObject serverStartedText;
    [SerializeField] private PlayerConnectionManager playerConnectionManager;
    [SerializeField] private SinglePlayerControlScheme singlePlayerControlScheme;
    [SerializeField] private NetworkControlScheme networkControlScheme;
    private ControlScheme controlScheme;

    private void Start() {

        startClient.onClick.AddListener(() => {
            StartNetworkClient();
        });
        startServer.onClick.AddListener(() => {
            StartServer();
        });
        startSinglePlayer.onClick.AddListener(() => {
            StartSinglePlayer();
        });
    }

    private void StartNetworkClient()
    {
        NetworkManager.Singleton.StartClient();
        controlScheme = networkControlScheme;
        StartClient();
    }

    private void StartServer()
    {
        serverStartedText.SetActive(true);
        startClient.gameObject.SetActive(false);
        startServer.gameObject.SetActive(false);
        startSinglePlayer.gameObject.SetActive(false);
        NetworkManager.Singleton.StartServer();
        playerConnectionManager.Initialize();
    }

    private void StartSinglePlayer()
    {
        controlScheme = singlePlayerControlScheme;
        StartClient();
    }

    private void StartClient()
    {
        game.SetActive(true);
        gameObject.SetActive(false);
        controlScheme.Initialize();
    }
}
