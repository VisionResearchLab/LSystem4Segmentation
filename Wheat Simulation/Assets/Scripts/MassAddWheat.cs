using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MassAddWheat : MonoBehaviour
{
    // The wheat 3D model to randomly place
    public GameObject wheat;

    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // Boundaries of wheat growth
    public GameObject boundary1;
    public GameObject boundary2;

    // Range of coordinates on the x axis where wheat can appear
    private float xMin;
    private float xMax;

    // Range of coordinates on the y axis where wheat can appear
    private float yMin;
    private float yMax;

    // Range of coordinates on the z axis where wheat can appear
    private float zMin;
    private float zMax;

    private void Start(){
        defineBoundaries();
    }

    // Finds the positions of the boundary transforms, and ensures that wheat will only spawn within these boundaries.
    private void defineBoundaries()
    {
        Transform transform1 = boundary1.GetComponent<Transform>();
        Transform transform2 = boundary2.GetComponent<Transform>();

        xMin = transform1.position.x;
        yMin = transform1.position.y;
        zMin = transform1.position.z;

        xMax = transform2.position.x;
        yMax = transform2.position.y;
        zMax = transform2.position.z;
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void InstantiateWheat()
    {
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);
        for (int i = 0; i < quantityToPlace; i++){
            Instantiate(wheat, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        }
    }
}
