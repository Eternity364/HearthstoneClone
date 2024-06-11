using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class NetworkControlScheme : NetworkBehaviour, ControlScheme {
    [SerializeField]
    CardGenerator cardGenerator;
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    PlayerConnectionManager playerConnectionManager;
    [SerializeField]
    ActiveCardController activeCardController;
    [SerializeField]
    Hand playerHand;
    [SerializeField]
    Hand opponentHand;
    [SerializeField]
    GameInstanceManager gameInstanceManager;
    [SerializeField]
    Button EndTurnButton;
    [SerializeField]
    EndTurnTimer endTurnTimer;

    private Card attacker, target;
    private Queue<InputBlock> blocks = new Queue<InputBlock>();

    BoardManager ControlScheme.bManager
    {
        get
        {
            return boardManager;
        }
    }
    ActiveCardController ControlScheme.ActiveCardController
    {
        get
        {
            return activeCardController;
        }
    }

    public void AddInputBlock() {
        InputBlock block = InputBlockerInstace.Instance.AddBlock();
        blocks.Enqueue(block);
    }

    private void DequeueInputBlock() {
        if (blocks.Count > 0) {
            InputBlock block = blocks.Dequeue();
            InputBlockerInstace.Instance.RemoveBlock(block);
        }
    }

    private bool CheckGameStateIntegrity(byte[] clientStateHash, byte[] serverStateHash, ulong clientID, GameInstance instance) {
        string serverHashString = SecurityHelper.GetHexStringFromHash(serverStateHash);
        string clientHashString = SecurityHelper.GetHexStringFromHash(clientStateHash);
        if (serverHashString == clientHashString) 
            return true;
        else
        {
            ClientRpcParams clientRpcParams = CreateClientRpcParams(clientID);
            ClientRpcParams opponentRpcParams = CreateClientRpcParams(instance.Pair.GetOpponentID(clientID));
            playerConnectionManager.SendClientShutDownClientRpc(false, clientRpcParams);
            playerConnectionManager.SendClientShutDownClientRpc(true, opponentRpcParams);
            gameInstanceManager.Remove(instance);
            StartCoroutine(playerConnectionManager.ShutDown());
            return false;
        }
    }

    void ControlScheme.Clear() {
        boardManager.OnCardAttack = null;
        boardManager.OnHeroAttack = null;
        boardManager.OnCardBattlecryBuff = null;
        activeCardController.OnCardDrop = null;
        blocks = new Queue<InputBlock>();
        attacker = null;
        target = null;
    }

    void ControlScheme.AttemptToPerformAttack(int attackerIndex, int targetIndex) {
        attacker = boardManager.PlayerCardsOnBoard[attackerIndex];
        target = boardManager.EnemyCardsOnBoard[targetIndex];
        boardManager.PlayerCardsOnBoard[attackerIndex].cardDisplay.SetActiveStatus(false);
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
        AttemptToPerformAttackServerRpc(attackerIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        AddInputBlock();
    }

    void ControlScheme.AttemptToPerformHeroAttack(int attackerIndex) {
        attacker = boardManager.PlayerCardsOnBoard[attackerIndex];
        GameStateInstance.Instance.AttackHero(PlayerState.Enemy, attackerIndex);
        boardManager.PlayerCardsOnBoard[attackerIndex].cardDisplay.SetActiveStatus(false);
        AttemptToPerformHeroAttackServerRpc(attackerIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        AddInputBlock();
    }

    void ControlScheme.AttemptToPerformBattlecryBuff(int casterIndex, int targetIndex) {
        GameStateInstance.Instance.ApplyBuff(PlayerState.Player, casterIndex, targetIndex);
        AttemptToPerformBattlecryBuffServerRpc(casterIndex, targetIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        AddInputBlock();
    }

    void ControlScheme.AttemptToPerformCardPlacement(int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
        AttemptToPerformCardPlacementServerRpc(handIndex, boardIndex, GameStateInstance.Instance.GetHash(), new ServerRpcParams());
        Card card = activeCardController.pickedCard;
        if (!card.GetData().abilities.Contains(Ability.BattlecryBuff))
            AddInputBlock();
    }

    void ControlScheme.AttemptToStartNextTurn() {
        AddInputBlock();
        EndTurnButton.interactable = false;
        EndTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enemy turn";
        endTurnTimer.gameObject.SetActive(false);
        AttemptToStartNextTurnServerRpc(GameStateInstance.Instance.GetHash(), new ServerRpcParams());
    }

    void ControlScheme.Concede() {
        boardManager.OnPreGameEnd();
        boardManager.playerHero.DealDamage(30);
        ConcedeServerRpc(new ServerRpcParams());
    }

    public void SendTimerStartMessage(GameInstance instance) {
        ulong playerId = instance.Pair.PlayerID;
        ulong opponentId = instance.Pair.EnemyID;
        
        ClientRpcParams playerRpcParams = CreateClientRpcParams(playerId);
        ClientRpcParams opponentRpcParams = CreateClientRpcParams(opponentId);
        
        TimerStartClientRpc(playerRpcParams);
        TimerStartClientRpc(opponentRpcParams);
    }
    
    public void ForceEndTurn(GameInstance instance) {
        PlayerState currentTurn = instance.currentTurn;
        ulong currentId = instance.Pair.GetIdByPlayerState(currentTurn);
        ulong nextId = instance.Pair.GetOpponentID(currentId);
        instance.currentTimer = 0;
        instance.currentTurn = PlayerState.Enemy;
        if (currentTurn == PlayerState.Enemy)
            instance.currentTurn = PlayerState.Player;

        
        ClientRpcParams currentRpcParams = CreateClientRpcParams(currentId);
        ClientRpcParams nextRpcParams = CreateClientRpcParams(nextId);
        PlayerState nextTurn = PlayerState.Player;
        if (currentTurn == PlayerState.Player) {
            nextTurn = PlayerState.Enemy;
        }

        instance.SetTurn(nextTurn);
        List<CardData> cardsInHand = instance.GameState.GetHandListByState(nextTurn);
        int newCardIndex = instance.cardIndexGeneratedThisTurn;

        GameState state = instance.GameState;
        if (currentTurn == PlayerState.Player) {
            state = state.GetReveresed();
        }

        SetNewTurnClientRpc(state.GetHash(), true, newCardIndex, nextRpcParams);
        SetNewTurnClientRpc(state.GetReveresed().GetHash(), false, newCardIndex, currentRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ConcedeServerRpc(ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ulong opponentID = instance.Pair.GetOpponentID(rpcParams.Receive.SenderClientId);
            ClientRpcParams opponentRpcParams = CreateClientRpcParams(opponentID);
            playerConnectionManager.SendClientShutDownClientRpc(true, opponentRpcParams);
            gameInstanceManager.Remove(instance);
        }
        StartCoroutine(playerConnectionManager.ShutDown());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToStartNextTurnServerRpc(byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ulong opponentID = instance.Pair.GetOpponentID(rpcParams.Receive.SenderClientId);
            PlayerState state = PlayerState.Enemy;
            if (instance.Pair.PlayerID == opponentID) {
                state = PlayerState.Player;
            }
            GameState gameState = instance.GameState;
            if (instance.Pair.PlayerID == opponentID) {
                gameState = instance.GameState.GetReveresed();
            }
            byte[] hash = gameState.GetHash();
            instance.SetTurn(state);
            if (instance.Pair.PlayerID == opponentID) {
                gameState = instance.GameState.GetReveresed();
            }
            int newCardIndex = instance.cardIndexGeneratedThisTurn;
            
            ClientRpcParams playerRpcParams = CreateClientRpcParams(rpcParams.Receive.SenderClientId);
            ClientRpcParams opponentRpcParams = CreateClientRpcParams(opponentID);
                      
            if (CheckGameStateIntegrity(stateHash, hash, rpcParams.Receive.SenderClientId, instance)) { 
                SetNewTurnClientRpc(gameState.GetHash(), false, newCardIndex, playerRpcParams);
                SetNewTurnClientRpc(gameState.GetReveresed().GetHash(), true, newCardIndex, opponentRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformBattlecryBuffServerRpc(int casterIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            PlayerState casterState = PlayerState.Player;
            if (!instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId)) {
                casterState = PlayerState.Enemy;
            }
            
            instance.GameState.ApplyBuff(casterState, casterIndex, targetIndex);
            
            GameState state = instance.GameState;
            if (casterState == PlayerState.Enemy) {
                state = instance.GameState.GetReveresed();
                ClientRpcParams tempParams = playerRpcParams;
                playerRpcParams = enemyRpcParams;
                enemyRpcParams = tempParams;
            }

            if (CheckGameStateIntegrity(stateHash, state.GetHash(), rpcParams.Receive.SenderClientId, instance)) {         
                print(state.GetReveresed().ToJson());
                PerformBattlecryBuffClientRpc(true, casterIndex, targetIndex, state.GetHash(), playerRpcParams);
                PerformBattlecryBuffClientRpc(false, casterIndex, targetIndex, state.GetReveresed().GetHash(), enemyRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformAttackServerRpc(int attackerIndex, int targetIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            bool attackerIsPlayer = true;
            if (!instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId))
                attackerIsPlayer = false;

            PlayerState attackerState = PlayerState.Player;
            PlayerState targetState = PlayerState.Enemy;
            if (!attackerIsPlayer)
            {
                attackerState = PlayerState.Enemy;
                targetState = PlayerState.Player;
            }
            
            instance.GameState.Attack(attackerState, attackerIndex, targetState, targetIndex);
            ClientRpcParams attackerRpcParams = playerRpcParams;
            ClientRpcParams targetRpcParams = enemyRpcParams;
            GameState state = instance.GameState;
            if (!attackerIsPlayer) {
                state = instance.GameState.GetReveresed();
                attackerRpcParams = enemyRpcParams;
                targetRpcParams = playerRpcParams;
            }
            if (CheckGameStateIntegrity(stateHash, state.GetHash(), rpcParams.Receive.SenderClientId, instance)) {        
                PerformAttackerMoveClientRpc(attackerRpcParams);
                PerformTargetMoveClientRpc(attackerIndex, targetIndex, state.GetReveresed().GetHash(), targetRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformHeroAttackServerRpc(int attackerIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            bool attackerIsPlayer = true;
            if (!instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId))
                attackerIsPlayer = false;

            PlayerState targetState = PlayerState.Enemy;
            if (!attackerIsPlayer)
            {
                targetState = PlayerState.Player;
            }
            
            instance.GameState.AttackHero(targetState, attackerIndex);
            ClientRpcParams attackerRpcParams = playerRpcParams;
            ClientRpcParams targetRpcParams = enemyRpcParams;
            GameState state = instance.GameState;
            if (!attackerIsPlayer) {
                state = instance.GameState.GetReveresed();
                attackerRpcParams = enemyRpcParams;
                targetRpcParams = playerRpcParams;
            }
            string serverHash = state.GetStringHash();
            string clientHash = SecurityHelper.GetHexStringFromHash(stateHash);
            if (CheckGameStateIntegrity(stateHash, state.GetHash(), rpcParams.Receive.SenderClientId, instance)) {           
                PerformAttackerHeroMoveClientRpc(attackerRpcParams);
                PerformTargetHeroMoveClientRpc(attackerIndex, state.GetReveresed().GetHash(), targetRpcParams);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptToPerformCardPlacementServerRpc(int handIndex, int boardIndex, byte[] stateHash, ServerRpcParams rpcParams) {
        GameInstance instance = gameInstanceManager.GetInstanceByPlayerID(rpcParams.Receive.SenderClientId);
        if (instance != null) {
            ClientRpcParams playerRpcParams = CreateClientRpcParams(instance.Pair.PlayerID);
            ClientRpcParams enemyRpcParams = CreateClientRpcParams(instance.Pair.EnemyID);

            bool IsPlayer = instance.Pair.IsClientIdPlayer(rpcParams.Receive.SenderClientId);

            PlayerState state = PlayerState.Player;
            if (!IsPlayer)
            {
                state = PlayerState.Enemy;
            }
            
            instance.GameState.PlaceCard(state, handIndex, boardIndex);
            ClientRpcParams placementClientRpcParams = enemyRpcParams;
            ClientRpcParams controlReleaseClientRpcParams = playerRpcParams;
            GameState gameState = instance.GameState;
            if (!IsPlayer) {
                gameState = instance.GameState.GetReveresed();
                placementClientRpcParams = playerRpcParams;
                controlReleaseClientRpcParams = enemyRpcParams;
            }
            
            print(gameState.ToJson());
            if (CheckGameStateIntegrity(stateHash, gameState.GetHash(), rpcParams.Receive.SenderClientId, instance)) {     
                PerformCardPlacementClientRpc(handIndex, boardIndex, gameState.GetReveresed().GetHash(), placementClientRpcParams);
                PerformControlReleaseClientRpc(controlReleaseClientRpcParams);
            }
        }
    }

    [ClientRpc]
    private void SetNewTurnClientRpc(byte[] stateHash, bool value, int newCardIndex, ClientRpcParams rpcParams) {
        PlayerState turn = PlayerState.Player;
        Hand hand = playerHand;
        if (!value) {
            turn = PlayerState.Enemy;
            hand = opponentHand;
        }
        GameStateInstance.Instance.SetCardsActive(turn);
        GameStateInstance.Instance.ProgressMana(turn);
        if (newCardIndex != -1) {
            Card card = cardGenerator.Create(newCardIndex, hand.transform);
            CardData cardData = card.GetData();
            CardData data = new CardData(cardData.Health, cardData.maxHealth, cardData.Attack, cardData.Cost, cardData.Index, cardData.abilities, cardData.buffs, cardData.battlecryBuff);
            GameStateInstance.Instance.GetHandListByState(turn).Add(data);
            hand.DrawCard(card);
        }

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();

        if (serverHash == clientHash) {
            if (value) {
                DequeueInputBlock();
                boardManager.OnPlayerTurnStart();
                
            }
            else {
                DequeueInputBlock();
                AddInputBlock();
                boardManager.SetCardsStatusActive(false);
            }
            EndTurnButton.interactable = true;
            EndTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End turn";
            endTurnTimer.gameObject.SetActive(false);
        }
    }
        
    [ClientRpc]
    private void PerformCardPlacementClientRpc(int handIndex, int boardIndex, byte[] stateHash, ClientRpcParams rpcParams) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Enemy, handIndex, boardIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash)
            opponentHand.PlaceCard(opponentHand.cards[handIndex], boardIndex);
    }
        
    [ClientRpc]
    private void PerformControlReleaseClientRpc(ClientRpcParams rpcParams) {
        DequeueInputBlock();
    }

    [ClientRpc]
    private void PerformAttackerMoveClientRpc(ClientRpcParams rpcParams) {
        boardManager.PerformAttackByCard(attacker, target);
        DequeueInputBlock();
    }

    [ClientRpc]
    private void PerformAttackerHeroMoveClientRpc(ClientRpcParams rpcParams) {
        PlayerState attakerState = PlayerState.Player;
        int index = boardManager.PlayerCardsOnBoard.IndexOf(attacker);
        boardManager.PerformHeroAttack(attakerState, index);
        DequeueInputBlock();
    }

    [ClientRpc]
    private void PerformTargetMoveClientRpc(int attackerIndex, int targetIndex, byte[] stateHash, ClientRpcParams rpcParams) {
        attacker = boardManager.EnemyCardsOnBoard[attackerIndex];
        target = boardManager.PlayerCardsOnBoard[targetIndex];
        GameStateInstance.Instance.Attack(PlayerState.Enemy, attackerIndex, PlayerState.Player, targetIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash)
            boardManager.PerformAttackByCard(attacker, target);
    }   

    [ClientRpc]
    private void PerformTargetHeroMoveClientRpc(int attackerIndex, byte[] stateHash, ClientRpcParams rpcParams) {
        PlayerState attackerState = PlayerState.Enemy;
        PlayerState targetState = PlayerState.Player;

        GameStateInstance.Instance.AttackHero(targetState, attackerIndex);

        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash) {}
            boardManager.PerformHeroAttack(attackerState, attackerIndex);
    }    
    

    [ClientRpc]
    private void PerformBattlecryBuffClientRpc(bool isPlayer, int casterIndex, int targetIndex, byte[] stateHash, ClientRpcParams rpcParams) {
        PlayerState state = PlayerState.Player;
        if (!isPlayer) {
            state = PlayerState.Enemy;
            GameStateInstance.Instance.ApplyBuff(state, casterIndex, targetIndex);
        }
        string serverHash = SecurityHelper.GetHexStringFromHash(stateHash);
        string clientHash = GameStateInstance.Instance.GetStringHash();
        if (serverHash == clientHash) {
            boardManager.PerformBattlecryBuff(state, casterIndex, targetIndex); 
            if (isPlayer) 
                DequeueInputBlock();
        }    
    }

    [ClientRpc]
    private void TimerStartClientRpc(ClientRpcParams rpcParams) {
        endTurnTimer.Begin();
        endTurnTimer.gameObject.SetActive(true);
    }

    private ClientRpcParams CreateClientRpcParams(ulong clientId) {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
    }
}
