using UnityEngine;

public class CardPickedRotationManager : MonoBehaviour
{
    private bool active = false;
    private float rotation = 0;
    private float speed = 1;
    private float rotationLimit = 30;
    private Vector3 previousPos;

    void Update()
    {
        if (active) {
            //rotation += speed;
            float posDiffX = previousPos.x - this.transform.localPosition.x;
            print(posDiffX);
            Rotate(posDiffX);
            previousPos = this.transform.localPosition;
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
        if (active) {
            previousPos = this.transform.localPosition;
            rotation = 0;
        }

    }

    public void Rotate(float posDiffX)
    {
        //print(posDiff);
        if (posDiffX < 0)
            rotation = -rotationLimit;
        else if(posDiffX > 0)
            rotation = rotationLimit;

        Quaternion qRotation = Quaternion.Euler(0, rotation, 0);
        this.transform.rotation = qRotation;
    }
}
