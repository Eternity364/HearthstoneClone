using UnityEngine;

public class CardPickedRotationManager : MonoBehaviour
{
    private bool active = false;
    private float rotation = 0;
    private float speed = 1;

    void Update()
    {
        if (active) {
            rotation += speed;
            Quaternion qRotation = Quaternion.Euler(0, rotation, 0);
            this.transform.rotation = qRotation;
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
    }
}
