using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Multiplay;
using Unity.Netcode;

public class ServerMultiplayManager : MonoBehaviour
{
    [SerializeField]
    MainMenuNetwork mainMenu;
    public delegate void StartServerDelegate();
    public StartServerDelegate StartServerHandler;

    public bool QueryProtocol = false;
    public bool Matchmaking = false;
    public bool Backfill = false;

    private string _allocationId;
    private int _maxPlayers = 2; // This can be hardcoded or dynamic based on your games requirements.
    private bool _sqpInitialized = false;

    private MultiplayEventCallbacks _multiplayEventCallbacks;
    private IServerEvents _serverEvents;
    private IServerQueryHandler _serverQueryHandler;

    private async void StartGame()
    {
        if (StartServerHandler == null)
        {
            Debug.Log("StartServerHandler is null, did you pass the correct method into Init?");
        }
        else
        {
            Debug.Log("Starting server...");

            StartServerHandler();

            // This example game doesn't have anything else to setup or assets to load,
            // inform multiplay we are ready to accept connections
            await MultiplayService.Instance.ReadyServerForPlayersAsync();
        }
    }

    // Unity Multiplay-matchmaking services calls this function once the server is ready.
    // At this point we can start the game, which means starting our network server.
    private void OnAllocate(MultiplayAllocation allocation)
    {
        Debug.Log("Server Allocated.");

        if (allocation != null)
        {
            if (string.IsNullOrEmpty(allocation.AllocationId))
            {
                Debug.LogError("Allocation id was null");
                return;
            }
            _allocationId = allocation.AllocationId;
            Debug.Log("Allocation id: " + _allocationId);

            if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening)
                return;

            StartGame();
        }
        else
        {
            Debug.LogError("Allocation was null");
        }
    }

    private async void InitializeSqp()
    {
        Debug.Log("InitializeSqp");

        try
        {
            // You can use these values setup match parameters.
            // Note: looks like this can be set with default values first, then updated
            // if the values come in later, which should be followed by a call to UpdateServerCheck.
            // maxPlayers is used to set the max players allowed on server.
            // DOCS: https://docs.unity.com/game-server-hosting/en/manual/sdk/game-server-sdk-for-unity#Start_server_query_handler
            // SDK 3
            _serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(
                (ushort)_maxPlayers,
                "DisplayName",
                "BADGameType",
                "0",
                "BADMap");

            // triggers an SDK call to UpdateServerCheck
            _sqpInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception why running StartServerQueryHandlerAsync: " + e.Message);
        }
    }

#if DEDICATED_SERVER
    private void Update()
    {
        // should just run once
        if (!_sqpInitialized) InitializeSqp();

        // This will constantly be called, and probably isn't the best approach.
        // The example matchplay project was being updated during the creation of this project, so
        // I've seen two different approahces, poll based and on value change:
        // - https://github.com/Unity-Technologies/com.unity.services.samples.matchplay/blob/master/Assets/Scripts/Matchplay/Server/Services/MultiplayServerQueryService.cs#L86
        // - https://github.com/Unity-Technologies/com.unity.services.samples.matchplay/blob/d1f30c65986a9cbafa8fa6b351d16133db4acc98/Assets/Scripts/Matchplay/Server/Services/MultiplayAllocationService.cs#L120
        if (_serverQueryHandler != null)
        {
            _serverQueryHandler.UpdateServerCheck(); // SDK 4
        }
    }
#endif

    public async void Init()
    {
        StartServerHandler = mainMenu.StartServer;

        // SDK 2
        _serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(_multiplayEventCallbacks);
    }

    private void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("MultiplayManager.Start");

        //Debug.Log(Application.targetFrameRate);
        //Debug.Log(QualitySettings.vSyncCount);

        // Fixes issue with excessive CPU, not sure if vSyncCount is necessary.
        // https://docs.unity.com/game-server-hosting/guides/troubleshooting.html#Servers_using_too_much_CPU
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        // Debug.Log(Application.targetFrameRate);
        // Debug.Log(QualitySettings.vSyncCount);

        _multiplayEventCallbacks = new MultiplayEventCallbacks();
        _multiplayEventCallbacks.Allocate += OnAllocate;
        _multiplayEventCallbacks.Deallocate += OnDeallocate;
        _multiplayEventCallbacks.Error += OnError;
        _multiplayEventCallbacks.SubscriptionStateChanged += OnSubscriptionStateChanged;

        Init();
#endif
    }

    private async void Awake()
    {
        Debug.Log("MultiplayManager.Awake");

        try
        {
            // Call Initialize async from SDK
            await UnityServices.InitializeAsync(); // SDK 1
        }
        catch (Exception e)
        {
            Debug.Log("InitializeAsync failed!");
            Debug.Log(e);
        }
    }

    public void UpdatePlayerCount(int count)
    {
        Debug.Log("UpdatePlayerCount: " + count);
        _serverQueryHandler.CurrentPlayers = (ushort)count;
    }

    /// <summary>
    /// Unready the server. This is to indicate that the server is in some condition which means it cannot accept players.
    /// For example, after a game has ended and you need to reset the server to prepare for a new match.
    /// </summary>
    public async Task UnReadyServer()
    {
        Debug.Log("UnReadyServer");
        await MultiplayService.Instance.UnreadyServerAsync();
        _serverEvents?.UnsubscribeAsync();
    }

    private void OnError(MultiplayError error)
    {
        Debug.Log("MultiplayManager OnError: " + error.Reason);
    }

    private void OnDeallocate(MultiplayDeallocation deallocation)
    {
        Debug.Log("Deallocated");
    }

    // DOCS: https://docs.unity.com/game-server-hosting/en/manual/sdk/game-server-sdk-for-unity#Handle_Game_Server_Hosting_events
    private void OnSubscriptionStateChanged(MultiplayServerSubscriptionState state)
    {
        switch (state)
        {
            case MultiplayServerSubscriptionState.Unsubscribed: /* The Server Events subscription has been unsubscribed from. */
                Debug.Log("Unsubscribed");
                break;
            case MultiplayServerSubscriptionState.Synced: /* The Server Events subscription is up to date and active. */
                Debug.Log("Synced");
                break;
            case MultiplayServerSubscriptionState.Unsynced: /* The Server Events subscription has fallen out of sync, the subscription will attempt to automatically recover. */
                Debug.Log("Unsynced");
                break;
            case MultiplayServerSubscriptionState.Error: /* The Server Events subscription has fallen into an errored state and will not recover automatically. */
                Debug.Log("Error");
                break;
            case MultiplayServerSubscriptionState.Subscribing: /* The Server Events subscription is attempting to sync. */
                Debug.Log("Subscribing");
                break;
        }
    }
}
