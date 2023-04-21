using UnityEngine;

public class PositionGetter : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    private float hitDistance = 200f;

    [SerializeField]
    private Collider coll;

    private static PositionGetter instance;
    public static Vector3 GetPosition () {
        Vector2 position = instance.GetBackgroundPosition();
        return position;
    }

    void Start () {
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        }
        else 
        { 
            instance = this; 
        } 
	}

    private Vector3 GetBackgroundPosition()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (coll.Raycast(ray, out hit, hitDistance))
        {
             return hit.point;
        }

        return Vector3.zero;
    }
}
