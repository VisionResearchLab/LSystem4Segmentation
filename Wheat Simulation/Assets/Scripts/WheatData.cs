using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatData : MonoBehaviour
{
    public Wheat.Part part;
    public String age;
    public Material material;
    public String materialName;

    private void Start()
    {
        // Get the material of this object
        material = gameObject.transform.GetComponent<Renderer>().material;
        materialName = material.name;
        
        // If the material is named after a part that exists, this object is one of those parts
        foreach (Wheat.Part enumPart in Enum.GetValues(typeof(Wheat.Part))){
            if (materialName.Contains(enumPart.ToString())){
                part = enumPart;
            }
        }

        // The age can be derived from the material name
        if (materialName.Contains("Young")){
            age = "Young";
        } else if (materialName.Contains("Middle")){
            age = "Middle";
        } else if (materialName.Contains("Mature")){
            age = "Mature";
        }
    }
    
    // Set the material of this object to the material corresponding to its part in Wheat.cs
    public void ToggleAnnotationOn(Dictionary<Wheat.Part, Material> partMaterialDict){
        gameObject.transform.GetComponent<Renderer>().material = partMaterialDict[part];
    }

    // Set the material of this object back to the original material
    public void ToggleAnnotationOff(){
        gameObject.transform.GetComponent<Renderer>().material = material;
    }
}
