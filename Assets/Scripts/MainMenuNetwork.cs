using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplay;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;

public class MainMenuNetwork : MonoBehaviour
{
    [SerializeField] private GameObject game;    
    [SerializeField] private TextMeshProUGUI matchIDtext;
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
    private IMatchmaker Matchmaker;
    private bool _endingGame = false;
    private const int k_TimeoutDuration = 10;
    
    void Awake()
    {
        Matchmaker = new ClientMatchplayMatchmaker();
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit()
    {
        Dispose();
    }

    public void Dispose()
    {
        Debug.Log("Dispose ServerNetworkManager");
        if (NetworkManager.Singleton == null)
            return;
#if DEDICATED_SERVER
        NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCheck;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        //NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnServerStarted -= OnNetworkReady;
#endif


        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    // NOTE: Function set to NetworkManager.ConnectionApprovalCallback.
    // Perform any approval checks required for you game here, set initial position and rotation, and whether or not connection is approved
    private void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;
        Debug.Log("Approval check for clientId: " + clientId);

        response.Approved = true;  // assumes all connections are valid

        // NOTE: post-connection logic cannot go here, the client is not connected here yet.
    }

    private async void SetupUnityServices()
    {
        try
        {
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // required to use Unity services like Matchmaker
            await UnityServices.InitializeAsync(); // Client SDK 1

            startClient.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log("UnityServices.InitializeAsync failed!");
            Debug.Log(e);
        }
    }

    private void Start() {
        startClient.onClick.AddListener(() => {
            controlScheme = networkControlScheme;
            startClient.gameObject.SetActive(false);
            waitingForOpponentText.SetActive(true);
            FindMatch();
        });
        // startServer.onClick.AddListener(() => {
        //     StartServer();
        // });
        // startSinglePlayer.onClick.AddListener(() => {
        //     StartSinglePlayer();
        // });


    playerConnectionManager.Initialize(StartClient, ShowWaitingForOpponentText);
#if DEDICATED_SERVER       
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
#else   
        SetupUnityServices();
#endif
    }

    void OnNetworkReady()
    {
        Debug.Log("server started!");
    }

    private void StartNetworkClient(MatchmakingResult matchmakingResult)
    {
        Debug.Log($"Connecting to server at {matchmakingResult.ip}:{matchmakingResult.port}");
        
        matchIDtext.SetText("Port: " + matchmakingResult.port.ToString());
        matchIDtext.gameObject.SetActive(true);
        controlScheme = networkControlScheme;

        // Set the server ip and port to connect to, received from our match making result.
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(matchmakingResult.ip, (ushort)matchmakingResult.port);


        // NOTE: For this demo no data is being sent upon connection, but you could do it with the example code below.
        // var userData = <some_user_data_source>;
        // var payload = JsonUtility.ToJson(userData);
        // var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        // _networkManager.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.NetworkConfig.ClientConnectionBufferTimeout = k_TimeoutDuration;

        //  If the socket connection fails, we'll hear back by getting an ReceiveLocalClientDisconnectStatus callback
        //  for ourselves and get a message telling us the reason. If the socket connection succeeds, we'll get our
        //  ReceiveLocalClientConnectStatus callback This is where game-layer failures will be reported.
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Starting Client!");
        }
        else
        {
            Debug.LogWarning($"Could not Start Client!");
        }
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
        endTurnButton.gameObject.SetActive(true);
        playerHand.Initialize(PlayerState.Player);
        opponentHand.Initialize(PlayerState.Enemy);
        boardManager.Initialize(isPlayer, OnPreGameEnd, OnGameEnd);
        controlScheme.Initialize();
        endTurnButton.interactable = isPlayer;
        if (isPlayer)
            endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End turn";
        else
            endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enemy turn";

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

    private async void FindMatch() {
        MatchmakingResult matchmakingResult = await FindMatchAsync();

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            // once we get the match ip and port we can connect to server
            StartNetworkClient(matchmakingResult);
        }
        else
        {
            Debug.Log("Failed to find match.");
        }
    }

    private async Task<MatchmakingResult> FindMatchAsync()
    {
        System.Random rnd = new System.Random();
        int num = rnd.Next();
        UserData mockUserData = new UserData("username" + num, "authid" + num, (ulong)num);

        Debug.Log($"Start matchmaking with {mockUserData}");
        MatchmakingResult matchmakingResult = await Matchmaker.FindMatch(mockUserData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            // Here you can see the matchmaker returns the server ip and port the client needs to connect to
            Debug.Log($"{matchmakingResult.result}, {matchmakingResult.resultMessage}, {matchmakingResult.ip}:{matchmakingResult.port}  ");
        }
        else
        {
            Debug.LogWarning($"{matchmakingResult.result} : {matchmakingResult.resultMessage}");
        }

        return matchmakingResult;
    }

    
    public void StartServer()
    {       
        Debug.Log($"Starting server on port {MultiplayService.Instance.ServerConfig.Port}");

        var unityTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;

        // set port based on server config
        unityTransport.SetConnectionData("0.0.0.0", (ushort)MultiplayService.Instance.ServerConfig.Port);

        NetworkManager.Singleton.StartServer();
    }
}
