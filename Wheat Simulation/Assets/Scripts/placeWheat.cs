using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlaceWheat : MonoBehaviour
{
    public GameObject wheat;

    public TMPro.TMP_InputField quantityToPlaceInputField;

    // Range of coordinates on the x axis where wheat can appear
    private float xMin = 0f;
    private float xMax = 100f;

    // Range of coordinates on the y axis where wheat can appear
    private float yMin = 0.8f;
    private float yMax = 1.2f;

    // Range of coordinates on the z axis where wheat can appear
    private float zMin = 0f;
    private float zMax = 100f;

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void InstantiateWheat()
    {
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);
        for (int i = 0; i < quantityToPlace; i++)
        Instantiate(wheat, new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax)), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
    }
}
