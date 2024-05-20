using UnityEngine;

public interface ControlScheme 
{
    abstract BoardManager bManager { get; }

    public void Initialize()
    {
        bManager.OnCardAttack += AttemptToPerformAttack;
    }

    void AttemptToPerformAttack(PlayerState state, int attackerIndex, int targetIndex);
}
