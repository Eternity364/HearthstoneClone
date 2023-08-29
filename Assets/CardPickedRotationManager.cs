using System;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;

public class CardPickedRotationManager : MonoBehaviour
{
    [SerializeField]
    AngleSetter angleSetter;

    private bool active = false;
    private float rotationX = 0;
    private float rotationY = 0;
    private float currentRotationX = 0;
    private float currentRotationY = 0;
    private float speed = 160;
    private float declineSpeedX = 2000;
    private float declineSpeedY = 1000;
    
    private float possDiffMin = 0.01f;
    private float possDiffMax = 0.07f;
    private float rotationLimitX = 35;
    private float rotationLimitY = 20;
    private Vector3 previousPos;
    float delayToZeroTween = 0;
    float currentDelayToZeroTweenX = 0f;
    float currentDelayToZeroTweenY = 0f;

    void Update()
    {
        if (active) {
            //rotation += speed;
            float posDiffX = previousPos.x - this.transform.localPosition.x;
            float posDiffY = previousPos.y - this.transform.localPosition.y;
            Rotate(posDiffX, posDiffY);
            previousPos = this.transform.localPosition;
            
            //print(angle);
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
        if (active) {
            previousPos = this.transform.localPosition;
            rotationX = 0;
            rotationY = 0;
        }

    }

    public void Rotate(float posDiffX, float posDiffY)
    {         
        void CalculateRotationForAxis(float posDiff, ref float currentRotation, ref float rotation, ref float currentDelayToZeroTween,
             float rotationLimit, float declineSpeed) {
                if (Math.Abs(posDiff) > possDiffMin) {
                    float rotationCoeff = Math.Abs(posDiff) / possDiffMax;
                    if (rotationCoeff > 1) rotationCoeff = 1;

                    float tempRotation = rotationCoeff * rotationLimit;
                    if (tempRotation > Math.Abs(rotation))
                        rotation = Math.Sign(posDiff) * tempRotation;
                
                    currentRotation += speed * Time.deltaTime * Math.Sign(posDiff);
                    if (Math.Sign(currentRotation) == Math.Sign(rotation) && Math.Abs(currentRotation) > Math.Abs(rotation))
                        currentRotation = rotation;

                    if (Math.Abs(currentRotation) > rotationLimit)
                        currentRotation = rotationLimit * Math.Sign(currentRotation);
                } 
                else
                {
                    int prevSign = Math.Sign(currentRotation);
                    float delta = declineSpeed * Time.deltaTime * Math.Abs(possDiffMax); 
                    if (Math.Abs(currentRotation) > delta)
                        currentRotation = (Math.Abs(currentRotation) - delta) * prevSign;
                    else
                        currentRotation = 0;
                    rotation = currentRotation;
                }
        }

        CalculateRotationForAxis(posDiffX, ref currentRotationX, ref rotationX, ref currentDelayToZeroTweenX, rotationLimitX, declineSpeedX);
        CalculateRotationForAxis(posDiffY, ref currentRotationY, ref rotationY, ref currentDelayToZeroTweenY, rotationLimitY, declineSpeedY);
        
        angleSetter.Set(new Vector2(-currentRotationY, currentRotationX));
    }
}
