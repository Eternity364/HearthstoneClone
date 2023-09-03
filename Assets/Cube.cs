using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField]
    public Transform childTransform;
    [SerializeField]
    public MeshRenderer renderer;
    [SerializeField]
    public Vector4 emissionColor;
}
