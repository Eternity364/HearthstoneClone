using UnityEngine;

public class PositionGetter : MonoBehaviour
{


    private RaycastHit hit;
    private Ray ray;
    private float hitDistance = 200f;

    public enum ColliderType
    {
        ActiveCardController,
        Background
    }

    [SerializeField]
    private Collider activeCardController;   
    [SerializeField]
    private Collider background;

    private static PositionGetter instance;
    public static Vector3 GetPosition (ColliderType collType) {
        Vector2 position = instance.GetBackgroundPosition(collType);
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

    private Vector3 GetBackgroundPosition(ColliderType collType)
    {
        Collider coll;
        if (collType == ColliderType.ActiveCardController)
            coll = activeCardController;
        else
            coll = background;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (coll.Raycast(ray, out hit, hitDistance))
        {
             return hit.point;
        }

        return Vector3.zero;
    }
}
