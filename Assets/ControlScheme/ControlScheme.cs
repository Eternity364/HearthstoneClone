using UnityEngine;

public interface ControlScheme 
{
    abstract BoardManager bManager { get; }
    abstract ActiveCardController ActiveCardController { get; }

    public void Initialize()
    {
        bManager.OnCardAttack += AttemptToPerformAttack;
        ActiveCardController.OnCardDrop += AttemptToPerformCardPlacement;
    }

    void AttemptToPerformAttack(PlayerState state, int attackerIndex, int targetIndex);
    void AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex);
    public void AttemptToStartNextTurn();
}
