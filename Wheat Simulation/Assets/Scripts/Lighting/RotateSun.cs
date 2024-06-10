using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSun : MonoBehaviour
{
    float minXRotation = 10f;
    float maxXRotation = 170f;

    float currentXRotation;

    [SerializeField] private bool orbiting = true;
    
    void Start(){
        currentXRotation = 75f;
    }

    void Update()
    {
        if (orbiting){
            currentXRotation += Time.deltaTime;
            if (currentXRotation >= maxXRotation){
                currentXRotation = minXRotation;
            }

            this.gameObject.transform.eulerAngles = new Vector3(currentXRotation, -270f, -180f);
        }
        
    }
}
