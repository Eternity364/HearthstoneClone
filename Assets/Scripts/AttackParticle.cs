using UnityEngine;

public class AttackParticle : MonoBehaviour
{
    [SerializeField]
    Transform cardTransform;
    
    public void SetAngle(Vector2 attackerPos)
    {
        Vector2 pos = cardTransform.position;
        Vector2 differance = pos - attackerPos;
        float angle = Vector2.Angle(differance, Vector3.up);
        if (differance.x > 0) {
            angle = -angle;
        }
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
