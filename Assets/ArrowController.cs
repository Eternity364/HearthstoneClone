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
    float finishVectorCutoofDistance = 0.3f;
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
    [SerializeField]
    GameObject arrow;
    [SerializeField]
    Transform arrowChildTransform;
    [SerializeField]
    float distanceAlphaCutoff;
    [SerializeField]
    float distanceAlphaTurnOff;
    [SerializeField]
    float positionAdjustment;

    Dictionary<Cube, float> cubeValues = new Dictionary<Cube, float>();
    
    [SerializeField]
    Vector3 finishPosition = Vector3.zero;
    Vector3 previousFinishPosition = Vector3.zero;
    Cube lastCube;
    bool active = false;
    bool alphaCutOff = false;

    public bool Active
    {
        get { return active; }
    }

    public void SetActive(bool active, Vector2 fromPosition) { 
        startPosition = fromPosition;
        this.active = active;
        if (!active && cubeValues.Count > 0) {
            foreach (Cube cube in cubeValues.Keys)
            {
                Destroy(cube.gameObject);
            }
            cubeValues = new Dictionary<Cube, float>();
        }
        arrow.gameObject.SetActive(active);
    }

    void Update() {        
        if (active) {
            finishPosition = PositionGetter.GetPosition(PositionGetter.ColliderType.Background);

            if (finishPosition == Vector3.zero)
                finishPosition = previousFinishPosition;

            if (cubeValues.Keys.Count == 0) {
                Cube cube = Instantiate(cubePrefab);
                cube.gameObject.SetActive(false);
                cube.transform.SetParent(this.transform);
                cubeValues[cube] = 0.9f;
                lastCube = cube;
            }
            if (lastCube)
                CalculateCubePositions();

            previousFinishPosition = finishPosition;
        }
    }

    void CalculateCubePositions() {
        List<Cube> cubes = new List<Cube>();
        foreach (Cube cube in cubeValues.Keys)
        {
            cubes.Add(cube);
        }
        cubes.Remove(lastCube);

        float maxDistance = (finishPosition - startPosition).magnitude;
        if (cubeValues[lastCube] < 0)
            cubeValues[lastCube] = 0.1f;
        float valueChange = cubeSpeed / (finishPosition - startPosition).magnitude;
        if (valueChange > maxValueChange)
            valueChange = maxValueChange;
        if (alphaCutOff && (maxDistance > distanceAlphaCutoff)) {
            cubeValues[lastCube] = 0.9f;
            alphaCutOff = false;
        }

        cubeValues[lastCube] += Time.deltaTime * valueChange;
        float currValue = cubeValues[lastCube];
        lastCube.gameObject.SetActive(true);
        SetCubeValues(lastCube);

        int index = cubes.Count;
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
            Vector3 cutoffFinishPosition = finishPosition - (finishPosition - startPosition).normalized * finishVectorCutoofDistance;
            Vector3 newVector = startPosition + (cutoffFinishPosition - startPosition) * cubeValues[cube];
            float maxDistance = (cutoffFinishPosition - startPosition).magnitude;
            float currentDistance = (cutoffFinishPosition - newVector).magnitude;
            
            float x = 1;
            if ((maxDistance - currentDistance) < 0.5f * maxDistance) {
                x = -1;
            }

            float value = cubeValues[cube];
            newVector.z = zCurve.Evaluate(value) * zMax * Math.Abs(startPosition.x) * positionAdjustment;
            cube.transform.localPosition = newVector;

            float angleZ = 180 - Vector3.Angle(cutoffFinishPosition - startPosition, Vector3.right) + 90;
            float angleX = startXAngle * x * xCurve.Evaluate(value);
            Vector3 differance = cutoffFinishPosition - startPosition;
            if (differance.y > 0) {
                angleZ = -angleZ;
                angleX = -angleX;
            }
            cube.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleZ));
            cube.childTransform.localRotation = Quaternion.Euler(new Vector3(angleX, 0, 0));
            Color color = cube.renderer.material.color;

            float distance = (finishPosition - startPosition).magnitude;
            if (distance < distanceAlphaCutoff) {
                value /= distance / distanceAlphaCutoff;
                if (value > 1) value = 1;
                alphaCutOff = true;
            } 
            cube.childTransform.gameObject.SetActive(value < 1);
            cube.childTransform.gameObject.SetActive(distance > distanceAlphaTurnOff);
            Vector3 childPosition = cube.childTransform.localPosition;
            cube.childTransform.localPosition = childPosition;
            float alpha = alphaCurve.Evaluate(value);
            cube.renderer.material.SetVector("_EmissionColor", cube.emissionColor * alpha);
            color.a = alpha;
            cube.renderer.material.color = color;

            if (cube == lastCube) {
                Vector3 newPosition = finishPosition;


                Vector3 childPos = arrowChildTransform.localPosition;
                
                if (differance.y > 0) {
                    angleZ += 180;
                }        
                childPos.x = -childPos.x;
                
                arrow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleZ));
                arrow.transform.localPosition = newPosition;
            }
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
