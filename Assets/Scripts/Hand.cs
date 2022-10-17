using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField]
    Card[] cards;
    [SerializeField]
    Vector3 positionShift;
    [SerializeField]
    Vector3 fanSortingStartPosition;
    [SerializeField]
    float fanSortingAngleShift;

    void Start()
    {
        SortFan();
    }

    public void SortLinear()
    {
        int lenght = cards.Length;
        Vector3 startPosition = (-lenght / 2 + 1) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= 0.5f * positionShift;

        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.position += startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderOrder(lenght - i);
        }
    }

    public void SortFan()
    {
        int lenght = cards.Length - 1;
        float angleShift = lenght;
        float startAngle = (-lenght / 2 + 0.5f) * fanSortingAngleShift;
        if (lenght % 2 == 1)
            startAngle -= 0.5f * fanSortingAngleShift;

        for (int i = 0; i < lenght; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, startAngle + (fanSortingAngleShift * i));
            Quaternion cardRotation = Quaternion.Euler(0, 0, (startAngle + (fanSortingAngleShift * i)) * 0.5f);
            Vector3 position = rotation * fanSortingStartPosition - fanSortingStartPosition;
            cards[i].transform.position += position;
            cards[i].transform.rotation = cardRotation;
            cards[i].cardDisplay.SetRenderOrder(lenght - i);
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
