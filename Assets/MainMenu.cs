using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using TMPro;

public class MainMenuLocal : MonoBehaviour
{
    [SerializeField] private GameObject game;
    [SerializeField] private Button startClient;
    [SerializeField] private Button startServer;
    [SerializeField] private Button startSinglePlayer;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject serverStartedText;
    [SerializeField] private GameObject waitingForOpponentText;
    [SerializeField] private PlayerConnectionManager playerConnectionManager;
    [SerializeField] private SinglePlayerControlScheme singlePlayerControlScheme;
    [SerializeField] private NetworkControlScheme networkControlScheme;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand opponentHand;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button concedeButton;
    [SerializeField] private ManaController playerManaController;
    [SerializeField] private ManaController enemyManaController;
    [SerializeField] private CardGenerator cardGenerator;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private InputBlocker inputBlocker;
    [SerializeField] private SplashScreen splashScreen;
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
        controlScheme = networkControlScheme;
        NetworkManager.Singleton.StartClient();
        startClient.gameObject.SetActive(false);
        startServer.gameObject.SetActive(false);
        startSinglePlayer.gameObject.SetActive(false);
        waitingForOpponentText.SetActive(true);
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
        GameState gameState = new GameState(cardGenerator.GetRandomDataList(0), cardGenerator.GetRandomDataList(0),
            cardGenerator.GetRandomDataList(5), cardGenerator.GetRandomDataList(5), 
            1, 0, 1, 0, 10, 10,
            30, 30, 30, 30,
            boardManager.OnCardDead, OnManaChange, OnHeroDead);
        StartClient(true, gameState.ToJson());
    }

    private void StartClient(bool isPlayer, string gameStateJson)
    {
        gameState = JsonUtility.FromJson<GameState>(gameStateJson);
        gameState.OnCardDead = boardManager.OnCardDead;
        gameState.OnManaChange = OnManaChange;
        gameState.OnHeroDead = OnHeroDead;
        GameStateInstance.SetInstance(gameState);;

        
        InputBlockerInstace.SetInstance(inputBlocker);
        InputBlockerInstace.Instance.Clear();
        boardManager.Clear();
        playerHand.Clear();
        opponentHand.Clear();
        controlScheme.Clear();
        splashScreen.Clear();
        endTurnButton.onClick.RemoveAllListeners();
        concedeButton.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
        game.SetActive(true);
        playerHand.Initialize(PlayerState.Player);
        opponentHand.Initialize(PlayerState.Enemy);
        boardManager.Initialize(isPlayer, OnPreGameEnd, OnGameEnd);
        controlScheme.Initialize();
        endTurnButton.interactable = isPlayer;
        if (isPlayer)
            endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End turn";
        else
            endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enemy turn";
        endTurnButton.gameObject.SetActive(true);
        endTurnButton.onClick.AddListener(controlScheme.AttemptToStartNextTurn);
        concedeButton.onClick.AddListener(controlScheme.Concede);
        gameCanvas.SetActive(true);

        if (!isPlayer) {
            NetworkControlScheme netControl = (NetworkControlScheme)controlScheme;
            netControl.AddInputBlock();
        } 
        else
        {
            boardManager.SetCardsStatusActive(true);
        }
        gameState.Update();
    }

    private void OnPreGameEnd() {
        gameCanvas.SetActive(false);
        InputBlockerInstace.Instance.AddBlock();
    }

    private void OnGameEnd() {
        game.SetActive(false);
        gameObject.SetActive(true);
        NetworkManager.Singleton.Shutdown();
        waitingForOpponentText.SetActive(false);
        startClient.gameObject.SetActive(true);
        startServer.gameObject.SetActive(true);
        startSinglePlayer.gameObject.SetActive(true);
    }

    private void OnManaChange(PlayerState state, int currentMana, int mana) {
        if (state == PlayerState.Player) {
            playerManaController.Set(currentMana, mana);
            playerHand.OnManaChange(state, currentMana, mana);
        } 
        else
        {
            enemyManaController.Set(currentMana, mana);
        }
    }

    private void OnHeroDead(PlayerState state) {

    }

    private void ShowWaitingForOpponentText()
    {
        waitingForOpponentText.SetActive(true);
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
        GameStateInstance.instance =  instance;
    }
}

public enum PlayerState : int
{
    Player = 0,
    Enemy = 1,
}
