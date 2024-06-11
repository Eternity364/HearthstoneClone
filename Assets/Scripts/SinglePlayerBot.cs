using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerBot : MonoBehaviour
{
    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    Hand opponentHand;
    [SerializeField]
    SinglePlayerControlScheme controlScheme;

    public void StartNewTurn() {
        StartCoroutine(WaitForNewAction());
    }

    IEnumerator WaitForNewAction()
    {
        yield return new WaitForSeconds(2.5f);

        bool doNextAction = ExecuteAction();
        if (doNextAction)
            StartCoroutine(WaitForNewAction());
        else {
            yield return new WaitForSeconds(2.5f);
            controlScheme.StartPlayerTurn();
        }
    }

    private bool ExecuteAction() {
        Card card = opponentHand.GetPlayableOpponentCard();
        if (card && !boardManager.IsOpponentFilled) {
            List<Card> cardsOnBoard = new List<Card>();
            for (int i = 0; i <  boardManager.EnemyCardsOnBoard.Count; i++)
            {
                cardsOnBoard.Add(boardManager.EnemyCardsOnBoard[i]);
            }
            int index = UnityEngine.Random.Range(0, boardManager.EnemyCardsOnBoard.Count);
            GameStateInstance.Instance.PlaceCard(PlayerState.Enemy, opponentHand.cards.IndexOf(card), index);
            opponentHand.PlaceCard(card, index);
            if (card.GetData().abilities.Contains(Ability.BattlecryBuff) && cardsOnBoard.Count > 0) {
                index = UnityEngine.Random.Range(0, cardsOnBoard.Count - 1);
                int casterIndex =  boardManager.EnemyCardsOnBoard.IndexOf(card);
                int targetIndex = boardManager.EnemyCardsOnBoard.IndexOf(cardsOnBoard[index]);
                GameStateInstance.Instance.ApplyBuff(PlayerState.Enemy, casterIndex, targetIndex);
                boardManager.PerformBattlecryBuff(PlayerState.Enemy,
                    casterIndex,
                    targetIndex);
            }
            return true;
        }

        int attackerIndex = -1;
        List<CardData> list = GameStateInstance.Instance.opponentCardsData;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Active) {
                attackerIndex = i;
                break;
            }
        }

        if (attackerIndex != -1 && boardManager.PlayerCardsOnBoard.Count > 0) {
            int targetIndex = UnityEngine.Random.Range(0, boardManager.PlayerCardsOnBoard.Count - 1);
            boardManager.PerformAttackByIndex(false, attackerIndex, targetIndex);
            GameStateInstance.Instance.Attack(PlayerState.Enemy, attackerIndex, PlayerState.Player, targetIndex);
            return true;
        }

        if (attackerIndex != -1 && boardManager.playerHero.data.Health > 0) {
            int targetIndex = UnityEngine.Random.Range(0, boardManager.PlayerCardsOnBoard.Count - 1);
            boardManager.PerformHeroAttack(PlayerState.Enemy, attackerIndex);
            GameStateInstance.Instance.AttackHero(PlayerState.Player, attackerIndex);
            return true;
        }

        return false;
    }
}