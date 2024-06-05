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
        GameStateInstance.Instance.Attack(PlayerState.Player, attackerIndex, PlayerState.Enemy, targetIndex);
    }

    void ControlScheme.AttemptToPerformBattlecryBuff(PlayerState state, int casterIndex, int targetIndex) {
        boardManager.PerformBattlecryBuff(PlayerState.Player, casterIndex, targetIndex);
        GameStateInstance.Instance.ApplyBuff(PlayerState.Player, casterIndex, targetIndex);
    }

    void ControlScheme.AttemptToPerformCardPlacement(PlayerState state, int handIndex, int boardIndex) {
        GameStateInstance.Instance.PlaceCard(PlayerState.Player, handIndex, boardIndex);
    }

    void ControlScheme.AttemptToStartNextTurn() {
    }
}
