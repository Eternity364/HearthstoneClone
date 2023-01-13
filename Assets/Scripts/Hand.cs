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
    float zPosiionShift;
    [SerializeField]
    int sortingTypeThreshold;
    [SerializeField]
    Vector3 fanSortingStartPosition;
    [SerializeField]
    float startAngle;
    [SerializeField]
    float endAngle;

    void Start()
    {
        int lenght = cards.Length;

        if (lenght > sortingTypeThreshold)
            SortFan(lenght);
        else
            SortLinear(lenght);


        for (int i = 0; i < lenght; i++)
        {
            float zPos = i * zPosiionShift;
            Vector3 origPos = cards[i].transform.position;
            // TEMP For some reason game adds -1 to z coordinate, so we temporarly offset it here
            Vector3 pos = new Vector3(origPos.x, origPos.y, zPos + 1);
            cards[i].transform.position = pos;
        }
    }

    public void SortLinear(int lenght)
    {
        Vector3 startPosition = (-lenght / 2 + 1) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= positionShift;

        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.position += startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderOrder(i);
        }
    }

    public void SortFan(int lenght)
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
            position.z = 0;
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
