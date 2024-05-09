using UnityEngine;

public interface ControlScheme 
{
    abstract BoardManager bManager { get; }

    public void Initialize()
    {
        bManager.OnCardAttack += AttemptToPerformAttack;
    }

    void AttemptToPerformAttack(bool attackerIsPlayer, int attackerIndex, int targetIndex);
}
