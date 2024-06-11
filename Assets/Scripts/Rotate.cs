using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 _rotation;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(_rotation * Time.deltaTime);
    }
}
