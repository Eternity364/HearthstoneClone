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

    void ControlScheme.AttemptToPerformAttack(PlayerState state, int attackerIndex, int targetIndex) {
        boardManager.PerformAttackByIndex(state == PlayerState.Player, attackerIndex, targetIndex);
    }
}
