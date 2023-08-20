using UnityEngine;

public class CardPickedRotationManager : MonoBehaviour
{
    [SerializeField]
    AngleSetter angleSetter;

    private bool active = false;
    private float rotation = 0;
    private float speed = 1;
    private float rotationLimit = 40;
    private Vector3 previousPos;

    void Update()
    {
        if (active) {
            //rotation += speed;
            float posDiffX = previousPos.x - this.transform.localPosition.x;
            Rotate(posDiffX);
            previousPos = this.transform.localPosition;
            
            //print(angle);
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

        angleSetter.Set(new Vector2(0, rotation));
    }
}
