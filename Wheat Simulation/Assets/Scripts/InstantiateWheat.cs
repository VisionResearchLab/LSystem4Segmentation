using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateWheat : MonoBehaviour
{
    public static InstantiateWheat IW;

    public GameObject wheatPrefab;
    
    // When the wheat is randomly generated, it will be shifted downward and rotated slightly.
    public float downTranslationMin = 0.1f;
    public float downTranslationMax = 0.25f;
    public float xRotationMax = 15f;
    public float yRotationMax = 360f;
    public float zRotationMax = 15f;

    void Awake(){
        if (IW = null){
            IW = this;
        } else {
            GameObject.Destroy(IW);
        }

        DontDestroyOnLoad(this);
    }

    public void GenerateWheat(Vector3 requestedPosition){
        // The wheat will be placed at a slightly lower position than requested, as wheat grows partly inside of the ground.
        float downTranslation = Random.Range(downTranslationMin, downTranslationMax);
        Vector3 position = requestedPosition - new Vector3(0, downTranslation, 0);

        float xRotation = Random.Range(-xRotationMax, xRotationMax);
        float yRotation = Random.Range(-yRotationMax, yRotationMax);
        float zRotation = Random.Range(-zRotationMax, zRotationMax);
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        Instantiate(wheatPrefab, position, rotation);
    }
}
