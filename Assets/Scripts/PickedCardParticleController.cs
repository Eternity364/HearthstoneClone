using UnityEngine;

public class PickedCardParticleController : MonoBehaviour
{
    Vector2 previousPos;

    void Update()
    {
        Vector2 pos = transform.position;
        if (previousPos != null && previousPos != pos) {
            Vector3 differance = previousPos - pos;
            float angle = Vector2.Angle(differance, Vector3.up);
            if (differance.x > 0) {
                angle = -angle;
            }
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        previousPos = pos;
    }
}
