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

    void ControlScheme.AttemptToPerformAttack(PlayerState state, int attackerIndex, int targetIndex) {
        boardManager.PerformAttackByIndex(state == PlayerState.Player, attackerIndex, targetIndex);
    }

    void ControlScheme.AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex) {
    }

    void ControlScheme.AttemptToStartNextTurn() {
    }
}
