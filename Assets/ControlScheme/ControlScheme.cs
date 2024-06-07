using UnityEngine;

public interface ControlScheme 
{
    abstract BoardManager bManager { get; }
    abstract ActiveCardController ActiveCardController { get; }

    public void Initialize()
    {
        bManager.OnCardAttack += AttemptToPerformAttack;
        bManager.OnHeroAttack += AttemptToPerformHeroAttack;
        bManager.OnCardBattlecryBuff += AttemptToPerformBattlecryBuff;
        ActiveCardController.OnCardDrop += AttemptToPerformCardPlacement;
    }

    void AttemptToPerformAttack(int attackerIndex, int targetIndex);
    void AttemptToPerformHeroAttack(int attackerIndex);
    void AttemptToPerformBattlecryBuff(int casterIndex, int targetIndex);
    void AttemptToPerformCardPlacement(int handIndex, int boardIndex);
    public void AttemptToStartNextTurn();
}
