using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerControlScheme : MonoBehaviour, ControlScheme {
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    ActiveCardController activeCardController;
    [SerializeField]
    Hand opponentHand;
    [SerializeField]
    Hand playerHand;
    [SerializeField]
    CardGenerator cardGenerator;
    [SerializeField]
    Button endTurnButton;
    [SerializeField]
    SinglePlayerBot singlePlayerBot;

    private InputBlock block = null;

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

    void ControlScheme.AttemptToPerformAttack(int attackerIndex, int targetIndex) {
        boardManager.PerformAttackByIndex(true, attackerIndex, targetIndex);
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
    }
    void ControlScheme.AttemptToPerformHeroAttack(int attackerIndex) {
        boardManager.PerformHeroAttack(PlayerState.Player, attackerIndex);
        GameStateInstance.Instance.AttackHero(PlayerState.Enemy, boardManager.PlayerCardsOnBoard[attackerIndex].GetData().Attack);
    }

    void ControlScheme.AttemptToPerformBattlecryBuff(int casterIndex, int targetIndex) {
        boardManager.PerformBattlecryBuff(PlayerState.Player, casterIndex, targetIndex);
        GameStateInstance.Instance.ApplyBuff(PlayerState.Player, casterIndex, targetIndex);
    }

    void ControlScheme.AttemptToPerformCardPlacement(int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
    }

    void ControlScheme.AttemptToStartNextTurn() {
        SetupTurn(PlayerState.Enemy, opponentHand);
        endTurnButton.interactable = false;
        endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Enemy turn";
        block = InputBlockerInstace.Instance.AddBlock();
        singlePlayerBot.StartNewTurn();
    }

    void ControlScheme.Concede() {
        boardManager.OnPreGameEnd();
        boardManager.playerHero.DealDamage(30);
    }

    public void StartPlayerTurn() {
        SetupTurn(PlayerState.Player, playerHand);
        endTurnButton.interactable = true;
        endTurnButton.GetComponentInChildren<TextMeshProUGUI>().text = "End turn";
        InputBlockerInstace.Instance.RemoveBlock(block);
        block = null;
        boardManager.OnPlayerTurnStart();
    }

    private void SetupTurn(PlayerState turn, Hand hand) {
        GameStateInstance.Instance.SetCardsActive(turn);
        GameStateInstance.Instance.ProgressMana(turn);
        if (GameStateInstance.Instance.GetHandListByState(turn).Count < 10) {
            Card card = cardGenerator.Create(cardGenerator.GetRandomData().Index, hand.transform);
            CardData cardData = card.GetData();
            CardData data = new CardData(cardData.Health, cardData.maxHealth, cardData.Attack, cardData.Cost, cardData.Index, cardData.abilities, cardData.buffs, cardData.battlecryBuff);
            GameStateInstance.Instance.GetHandListByState(turn).Add(data);
            hand.DrawCard(card);
        }
    }
}
