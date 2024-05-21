using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverTest : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        mainCamera = this.gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectWheatPart();
        }
    }

    void DetectWheatPart()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Renderer renderer = hit.transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                string materialName = renderer.material.name;
                // Remove (Instance) from the material name if necessary
                materialName = materialName.Replace(" (Instance)", "");
                
                Debug.Log("You are mousing over part with material: " + materialName);

                // Example of changing the color of the part
                renderer.material.color = Color.red; // Change color to red
            }
        }
    }
}
