using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickedCardParticleController : MonoBehaviour
{
    Vector2 previousPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = transform.position;
        if (previousPos != null && previousPos != pos) {
            Vector3 differance = previousPos - pos;
            float angle = Vector2.Angle(differance, Vector3.up);
            if (differance.x > 0) {
                angle = -angle;
            }
            print(angle);
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        previousPos = pos;
    }
}
