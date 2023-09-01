using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    float startXAngle;
    [SerializeField]
    float z = 0;
    [SerializeField]
    Cube cubePrefab;
    [SerializeField]
    Vector3 startPosition;
    [SerializeField]
    float zMax = 0.3f;
    [SerializeField]
    float cubeSpeed = 0.03f;
    [SerializeField]
    float delayBetweenCubes = 1f;
    [SerializeField]
    AnimationCurve xCurve;
    [SerializeField]
    AnimationCurve zCurve;

    Dictionary<Cube, float> cubeValues = new Dictionary<Cube, float>();
    float currentDelayBetweenCubes = 1f;
    Vector3 finishPosition1 = Vector3.zero;

    void Update() {
        currentDelayBetweenCubes -= Time.deltaTime;
        if (currentDelayBetweenCubes < 0) {
            Cube cube = Instantiate(cubePrefab);
            cube.gameObject.SetActive(true);
            cube.transform.SetParent(this.transform);
            cubeValues[cube] = 0;
            currentDelayBetweenCubes = delayBetweenCubes;
        }

        List<Cube> keys = new List<Cube>();
        foreach (Cube cube in cubeValues.Keys)
        {
            keys.Add(cube);
        }

        foreach (Cube cube in keys)
        {
            cubeValues[cube] += Time.deltaTime * cubeSpeed / (finishPosition1 - startPosition).magnitude;
            SetCubeValues(cube);
        }

        finishPosition1 = PositionGetter.GetPosition(PositionGetter.ColliderType.ActiveCardController);
    }

    void SetCubeValues(Cube cube) {
        if (cubeValues[cube] < 1) {
            Vector3 newVector = startPosition + (finishPosition1 - startPosition) * cubeValues[cube];
            float maxDistance = (finishPosition1 - startPosition).magnitude;
            float currentDistance = (finishPosition1 - newVector).magnitude;
            
            float z;
            float x = 1;
            if ((maxDistance - currentDistance) < 0.5f * maxDistance) {
                z = (maxDistance - currentDistance) / (0.5f * maxDistance);
                x = -1;
            }
            else {
                z = currentDistance / (0.5f * maxDistance);
            }
            newVector.z = zCurve.Evaluate(cubeValues[cube]) * zMax;
            cube.transform.localPosition = newVector;

            float angleZ = 180 - Vector3.Angle(finishPosition1 - startPosition, Vector3.right) + 90;
            float angleX = startXAngle * x * xCurve.Evaluate(cubeValues[cube]);
            Vector3 differance = finishPosition1 - startPosition;
            if (differance.y > 0) {
                angleZ = -angleZ;
                angleX = -startXAngle * x * xCurve.Evaluate(cubeValues[cube]);
            }
            print(angleZ);
            cube.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleZ));
            cube.childTransform.localRotation = Quaternion.Euler(new Vector3(angleX, 0, 0));
        }
        else
        {
            cubeValues.Remove(cube);
            Destroy(cube.gameObject); 
        }
    }
}
