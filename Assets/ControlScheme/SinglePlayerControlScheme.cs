using UnityEngine;

public class SinglePlayerControlScheme : MonoBehaviour, ControlScheme {
    [SerializeField]
    BoardManager boardManager;

    BoardManager ControlScheme.bManager
    {
        get
        {
            return boardManager;
        }
    }

    void ControlScheme.AttemptToPerformAttack(bool attackerIsPlayer, int attackerIndex, int targetIndex) {
        boardManager.PerformAttackByIndex(attackerIsPlayer, attackerIndex, targetIndex);
    }
}
