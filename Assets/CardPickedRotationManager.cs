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
    private float speed = 130;
    private float possDiffMax = 0.1f;
    private float rotationLimitX = 50;
    private float rotationLimitY = 20;
    private Vector3 previousPos;
    private Tweener toZeroTweenX = null;
    private Tweener toZeroTweenY = null;
    float tweenerSpeed = 0.5f;

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
        void CalculateRotationForAxis(float posDiff, ref float currentRotation, ref float rotation, float rotationLimit, Tweener toZeroTween) {
                if (toZeroTween != null) {
                    toZeroTween.Kill();
                    if (Math.Sign(posDiff) == Math.Sign(currentRotation))
                        rotation = currentRotation;
                    else
                        CalculateRotation(ref rotation);
                } 
                else 
                {
                    CalculateRotation(ref rotation);
                }

                void CalculateRotation (ref float rotation) {
                    float rotationCoeff = Math.Abs(posDiff) / possDiffMax;
                    if (rotationCoeff > 1) rotationCoeff = 1;

                    float tempRotation = rotationCoeff * rotationLimit;
                    if (tempRotation > Math.Abs(rotation))
                        rotation = Math.Sign(posDiff) * tempRotation;
                }
            
                currentRotation += speed * Time.deltaTime * Math.Sign(posDiff);
                if (Math.Sign(currentRotation) == Math.Sign(rotation) && Math.Abs(currentRotation) > Math.Abs(rotation))
                    currentRotation = rotation;

                if (Math.Abs(currentRotation) > rotationLimit)
                    currentRotation = rotationLimit * Math.Sign(currentRotation);
        }

        if (posDiffX != 0) {
            CalculateRotationForAxis(posDiffX, ref currentRotationX, ref rotationX, rotationLimitX, toZeroTweenX);
        }
        else if (toZeroTweenX == null && currentRotationX != 0)
        {   
            void NullTweener() {
                toZeroTweenX = null;
            }
            TweenCallback callback = NullTweener;

            toZeroTweenX = DOTween.To(()=> currentRotationX, x=> currentRotationX = x, 0, Math.Abs(currentRotationX) / rotationLimitX * tweenerSpeed);
            toZeroTweenX.SetEase(Ease.OutQuad).OnKill(callback).SetAutoKill(true);
        }
        
        if (posDiffY != 0) {
            CalculateRotationForAxis(posDiffY, ref currentRotationY, ref rotationY, rotationLimitY, toZeroTweenY);
        }
        else if (toZeroTweenY == null && currentRotationY != 0)
        {   
            void NullTweener() {
                toZeroTweenY = null;
            }
            TweenCallback callback = NullTweener;

            toZeroTweenY = DOTween.To(()=> currentRotationY, x=> currentRotationY = x, 0, Math.Abs(currentRotationY) / rotationLimitY * tweenerSpeed);
            toZeroTweenY.SetEase(Ease.OutQuad).OnKill(callback).SetAutoKill(true);
        }
        
        angleSetter.Set(new Vector2(-currentRotationY, currentRotationX));
    }
}
