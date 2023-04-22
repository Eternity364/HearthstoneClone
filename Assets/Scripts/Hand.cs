using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    [SerializeField]
    Card[] cards;
    [SerializeField]
    Vector3 positionShift;
    [SerializeField]
    float zPosiionShift;
    [SerializeField]
    int sortingTypeThreshold;
    [SerializeField]
    Vector3 fanSortingStartPosition;
    [SerializeField]
    float startAngle;
    [SerializeField]
    float endAngle;
    [SerializeField]
    ActiveCardController cardController;
    
    int lenght;

    void Start()
    {
        lenght = cards.Length;

        Sort();

        for (int i = 0; i < lenght; i++)
        {
            cards[i].clickHandler.OnPick += cardController.PickCard;
        }
    }

    public void Sort() {
        if (lenght > sortingTypeThreshold)
            SortFan();
        else
            SortLinear();
    }

    public void SortLinear()
    {
        Vector3 startPosition = (-lenght / 2 + 1) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= positionShift;

        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.localPosition += startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderOrder(i);
        }
    }

    public void SortFan()
    {
        float fanSortingAngleShift = (endAngle - startAngle) / lenght;
        float localStartAngle = startAngle;
        // if (lenght % 2 == 1)
            // localStartAngle += 0.5f * fanSortingAngleShift;

        for (int i = 0; i < lenght; i++)
        {
            float angle = localStartAngle + (fanSortingAngleShift * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Quaternion cardRotation = Quaternion.Euler(0, 0, (startAngle + (fanSortingAngleShift * i)) * 0.5f);
            Vector3 position = rotation * fanSortingStartPosition - fanSortingStartPosition;
            position.z = 0.001f * i;
            cards[i].transform.localPosition = position;
            cards[i].transform.rotation = cardRotation;
            cards[i].cardDisplay.SetRenderOrder(lenght - i);
        }
    }

    private void SetCardsClickable(bool active) {
        for (int i = 0; i < lenght; i++)
        {
            cards[i].clickHandler.SetClickable(active);
        }
    }

    Vector3 RotateTowardsUp(Vector3 start, float angle)
    {
        // // if you know start will always be normalized, can skip this step
        // start.Normalize();

        // Vector3 axis = Vector3.Cross(start, Vector3.up);

        // // handle case where start is colinear with up
        // if (axis == Vector3.zero) axis = Vector3.right;
        
        return Quaternion.Euler(0, 0, angle) * start;
    }
}
