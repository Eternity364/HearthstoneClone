using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    float startXAngle;
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
    float maxValueChange = 1f;
    [SerializeField]
    float distanceBetweenCubes = 0.1f;
    [SerializeField]
    AnimationCurve xCurve;
    [SerializeField]
    AnimationCurve zCurve;
    [SerializeField]
    AnimationCurve alphaCurve;

    Dictionary<Cube, float> cubeValues = new Dictionary<Cube, float>();
    float currentDelayBetweenCubes = 0f;
    Vector3 finishPosition1 = Vector3.zero;
    Vector3 previousFinishPosition = Vector3.zero;
    Cube lastCube;

    void Update() {        
        finishPosition1 = PositionGetter.GetPosition(PositionGetter.ColliderType.Background);
        if (finishPosition1 == Vector3.zero)
            finishPosition1 = previousFinishPosition;
        //print(finishPosition1);

        if (cubeValues.Keys.Count == 0) {
            Cube cube = Instantiate(cubePrefab);
            cube.gameObject.SetActive(false);
            cube.transform.SetParent(this.transform);
            cubeValues[cube] = 0.9f;
            lastCube = cube;
        }
        //print("lastCube = " + lastCube);
        if (lastCube)
            CalculateCubePositions();

        previousFinishPosition = finishPosition1;
    }

    void CalculateCubePositions() {
        List<Cube> cubes = new List<Cube>();
        foreach (Cube cube in cubeValues.Keys)
        {
            cubes.Add(cube);
        }
        cubes.Remove(lastCube);

        float maxDistance = (finishPosition1 - startPosition).magnitude;
        //float previousDistance = (previousFinishPosition - lastCube.transform.localPosition).magnitude;
        //cubeValues[lastCube] = 1 - (previousDistance / maxDistance);
        if (cubeValues[lastCube] < 0)
            cubeValues[lastCube] = 0.1f;
        float valueChange = cubeSpeed / (finishPosition1 - startPosition).magnitude;
        if (valueChange > maxValueChange)
            valueChange = maxValueChange;
        cubeValues[lastCube] += Time.deltaTime * valueChange;
        //print(valueChange);
        float currValue = cubeValues[lastCube];
        lastCube.gameObject.SetActive(true);
        // print("prev_dis - " + previousDistance);
        // print("max_dis - " + maxDistance);
        //print("cubeValues[lastCube] - " + cubeValues[lastCube]);
        //print(maxDistance);
        SetCubeValues(lastCube);
        //print(cubeValues[lastCube]);

        int index = cubes.Count;
        //currValue -= distanceBetweenCubes / maxDistance;
        while (currValue > 0) 
        {   
            index--;
            currValue -= distanceBetweenCubes / maxDistance;
            Cube cube;
            if (index >= 0) {
                cubeValues[cubes[index]] = currValue;
                cube = cubes[index];
            }
            else
            {
                cube = Instantiate(cubePrefab);
                cube.transform.SetParent(this.transform);
                cubeValues[cube] = currValue;
            }
            cube.gameObject.SetActive(true);

            if (currValue < 0) {
                while (index >= 0) {
                    Cube cube1 = cubes[index];
                    cube1.gameObject.SetActive(false);
                    index--;
                }
            }
        }

        foreach (Cube cube in cubes)
        {
            SetCubeValues(cube);
        }  
  
    }

    void SetCubeValues(Cube cube) {
        if (cubeValues[cube] < 1) {
            Vector3 newVector = startPosition + (finishPosition1 - startPosition) * cubeValues[cube];
            float maxDistance = (finishPosition1 - startPosition).magnitude;
            float currentDistance = (finishPosition1 - newVector).magnitude;
            
            float x = 1;
            if ((maxDistance - currentDistance) < 0.5f * maxDistance) {
                x = -1;
            }

            float value = cubeValues[cube];
            newVector.z = zCurve.Evaluate(value) * zMax;
            cube.transform.localPosition = newVector;

            float angleZ = 180 - Vector3.Angle(finishPosition1 - startPosition, Vector3.right) + 90;
            float angleX = startXAngle * x * xCurve.Evaluate(value);
            Vector3 differance = finishPosition1 - startPosition;
            if (differance.y > 0) {
                angleZ = -angleZ;
                angleX = -angleX;
            }
            cube.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleZ));
            cube.childTransform.localRotation = Quaternion.Euler(new Vector3(angleX, 0, 0));
            Color color = cube.renderer.material.color;
            color.a = 1 * alphaCurve.Evaluate(value);
            cube.renderer.material.SetVector("_EmissionColor", cube.emissionColor * alphaCurve.Evaluate(value));
            cube.renderer.material.color = color;
            if (cube == lastCube)
                 print(cube.renderer.material.color.a);
        }
        else
        {
            cubeValues.Remove(cube);
            cube.gameObject.SetActive(false);
            Destroy(cube.gameObject);
            lastCube = null;
            Cube maxValueCube = null;
            float maxValue = -1000;
            foreach (Cube cube1 in cubeValues.Keys)
            {
                if (maxValue < cubeValues[cube1]) {
                    maxValue = cubeValues[cube1];
                    maxValueCube = cube1;
                }
            }
            if (maxValueCube != null) {
                lastCube = maxValueCube;
            }
        }
    }
}
