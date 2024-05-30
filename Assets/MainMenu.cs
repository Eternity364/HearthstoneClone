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
    [SerializeField] private GameObject waitingForOpponentText;
    [SerializeField] private PlayerConnectionManager playerConnectionManager;
    [SerializeField] private SinglePlayerControlScheme singlePlayerControlScheme;
    [SerializeField] private NetworkControlScheme networkControlScheme;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private ManaController manaController;
    private ControlScheme controlScheme;
    private GameState gameState;
    

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

        playerConnectionManager.Initialize(StartClient, ShowWaitingForOpponentText);
    }

    private void StartNetworkClient()
    {
        NetworkManager.Singleton.StartClient();
        controlScheme = networkControlScheme;
    }

    private void StartServer()
    {
        serverStartedText.SetActive(true);
        startClient.gameObject.SetActive(false);
        startServer.gameObject.SetActive(false);
        startSinglePlayer.gameObject.SetActive(false);
        NetworkManager.Singleton.StartServer();
    }

    private void StartSinglePlayer()
    {
        controlScheme = singlePlayerControlScheme;
        StartClient(true);
    }

    private void StartClient(bool isPlayer)
    {
        game.SetActive(true);
        gameObject.SetActive(false);
        boardManager.Initialize(isPlayer);
        controlScheme.Initialize();
        
        int initialPlayerMana = 1;
        int initialOpponentMana = 0;
        if (!isPlayer) {
            initialPlayerMana = 0;
            initialOpponentMana = 1;
        }
        gameState = new GameState(boardManager.PlayerCardsOnBoard, boardManager.EnemyCardsOnBoard, playerHand.cards, opponentHand.cards, 
            initialPlayerMana, initialOpponentMana, 10, 10, boardManager.OnCardDead, OnManaChange);
        GameStateInstance.SetInstance(gameState);
        endTurnButton.onClick.AddListener(controlScheme.AttemptToStartNextTurn);
        endTurnButton.gameObject.SetActive(isPlayer);
        if (!isPlayer) {
            NetworkControlScheme netControl = (NetworkControlScheme)controlScheme;
            netControl.AddInputBlock();
        } 
        else
        {
            boardManager.SetCardsStatusActive(true);
        }
    }

    private void OnManaChange(PlayerState state, int currentMana, int Mana) {
        if (state == PlayerState.Player) {
            manaController.Set(currentMana, Mana);
        }
    }

    private void ShowWaitingForOpponentText()
    {
        waitingForOpponentText.SetActive(true);
        startClient.gameObject.SetActive(false);
        startServer.gameObject.SetActive(false);
        startSinglePlayer.gameObject.SetActive(false);
    }
}

public static class GameStateInstance
{
    private static GameState instance;
 
    public static GameState Instance
    {
        get { return instance; }
    }
 
    public static void SetInstance(GameState instance)
    {
        if (GameStateInstance.instance == null)
           GameStateInstance.instance =  instance;
    }
}

public enum PlayerState : int
{
    Player = 0,
    Enemy = 1,
}
