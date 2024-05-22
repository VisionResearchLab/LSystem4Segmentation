using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatData : MonoBehaviour
{
    public Part part;
    public String age;
    public Material material;
    public String materialName;

    public enum Part {
        Wheat,
        Head,
        Stem,
        Leaf
    }

    private void Start()
    {
        // Get the material
        material = gameObject.transform.GetComponent<Renderer>().material;
        materialName = material.name;
        
        // If the material is named after a part that exists, this object is one of those parts
        foreach (Part enumPart in Enum.GetValues(typeof(Part))){
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

    // Check if an obj is a wheat. If there is a second argument, also check if the obj is of the same part as the parameter.
    public static bool ObjectIsWheat(GameObject obj, Part part = Part.Wheat){
        WheatData wheatData = obj.GetComponent<WheatData>();
        if (wheatData){
            // If the user doesn't specify the part, but the object has a wheatData script, it is a wheat
            if (part == Part.Wheat){
                return true;
            }

            // If the user does specify the part, the wheatData.part must be the same, or return false
            if (wheatData.part == part){
                return true;
            }
        } 
        return false;
    }
}
