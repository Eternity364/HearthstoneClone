using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adjusts angle of card depending of position, set card's angle only via this tool.
public class AngleSetter : MonoBehaviour
{
    [SerializeField]
    Vector2 angleAdjust;

    public void Set(Vector2 rotation)
    {
        Quaternion qRotation = Quaternion.Euler(
            rotation.x + Mathf.Abs(rotation.x) * (this.transform.localPosition.y * angleAdjust.x),
            rotation.y + Mathf.Abs(rotation.y) * (this.transform.localPosition.x * angleAdjust.y),
            0);
        // Quaternion qRotation = Quaternion.Euler(
        //     rotation.x,
        //     rotation.y,
        //     0);
        print(Mathf.Abs(rotation.y) * (this.transform.localPosition.x * angleAdjust.y));
        //print(this.transform.localPosition.x);
        this.transform.rotation = qRotation;
    }
}
