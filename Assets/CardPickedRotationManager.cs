using System;
using UnityEngine;
using DG.Tweening;

public class CardPickedRotationManager : MonoBehaviour
{
    [SerializeField]
    AngleSetter angleSetter;

    private bool active = false;
    private float rotation = 0;
    private float currentRotation = 0;
    private float speed = 130;
    private float possDiffMax = 0.1f;
    private float rotationLimit = 60;
    private Vector3 previousPos;
    private Tweener toZeroTween;
    float tweenerSpeed = 0.75f;

    void Update()
    {
        if (active) {
            //rotation += speed;
            float posDiffX = previousPos.x - this.transform.localPosition.x;
            Rotate(posDiffX);
            previousPos = this.transform.localPosition;
            
            //print(angle);
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
        if (active) {
            previousPos = this.transform.localPosition;
            rotation = 0;
        }

    }

    public void Rotate(float posDiffX)
    {
        if (posDiffX != 0 && posDiffX > 0) {
            if (toZeroTween != null) {
                toZeroTween.Kill();
                rotation = currentRotation;
            } 
            else 
            {
                float rotationCoeff = Math.Abs(posDiffX) / possDiffMax;
                if (rotationCoeff > 1) rotationCoeff = 1;

                float tempRotation = rotationCoeff * rotationLimit;
                if (tempRotation > Math.Abs(rotation))
                    rotation = Math.Sign(posDiffX) + tempRotation;
            }
           
            currentRotation += speed * Time.deltaTime * Math.Sign(posDiffX);
            if (Math.Abs(currentRotation) > Math.Abs(rotation)) currentRotation = rotation;

        } 
        else if (toZeroTween == null && currentRotation != 0)
        {   
            // int sign = Math.Sign(currentRotation);
            // if (currentRotation != 0) {
            //     currentRotation += speed * Time.deltaTime * -Math.Sign(currentRotation);
            //     if (Math.Sign(currentRotation) != sign)
            //         currentRotation = 0;
            // }

            void NullTweener() {
                toZeroTween = null;
            }
            TweenCallback callback = NullTweener;

           toZeroTween = DOTween.To(()=> currentRotation, x=> currentRotation = x, 0, currentRotation / rotationLimit * tweenerSpeed);
           toZeroTween.SetEase(Ease.OutQuad).OnKill(callback).SetAutoKill(true);
        }
        print(currentRotation);
        angleSetter.Set(new Vector2(0, currentRotation));
    }
}
