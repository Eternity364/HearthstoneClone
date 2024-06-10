using UnityEngine;

public class SinglePlayerControlScheme : MonoBehaviour, ControlScheme {
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    ActiveCardController activeCardController;

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
    }

    void ControlScheme.Concede() {
        boardManager.OnPreGameEnd();
        boardManager.playerHero.DealDamage(30);
    }
}
