using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MassAddWheat : MonoBehaviour
{
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
        Vector3 pos1 = boundary1.GetComponent<Transform>().position;
        Vector3 pos2 = boundary2.GetComponent<Transform>().position;

        xMin = pos1.x;
        yMin = pos1.y;
        zMin = pos1.z;

        xMax = pos2.x;
        yMax = pos2.y;
        zMax = pos2.z;
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void AddWheat()
    {
        // Get the amount of wheat to place from the user input
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);

        for (int i = 0; i < quantityToPlace; i++){
            // X and Z positions are always assumed to be valid, because ground exists everywhere
            float wheatPosX = Random.Range(xMin, xMax);
            float wheatPosZ = Random.Range(zMin, zMax);

            // The Y position must be found by casting a ray downwards, in case the ground is not level
            Ray ray = new Ray(origin: new Vector3(wheatPosX, Mathf.Max(yMin, yMax), wheatPosZ), direction: new Vector3(0, -1, 0));
            RaycastHit hit;

            // Should only interact with the ground layer
            if(Physics.Raycast(ray, out hit, Mathf.Abs(yMax - yMin), Wheat.groundLayerMask)){
                InstantiateWheat.IW.GenerateWheat(hit.point);
            }
        }
    }
}
