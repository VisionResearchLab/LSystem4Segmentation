using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WheatData : MonoBehaviour
{
    // Variables are determined whenever they are requested, rather than being set on start, to prevent issues with model-->prefab conversion.

    // The part can be Stem, Head, Leaf, or generic Wheat
    public Wheat.Part part
    {
        get { 
            foreach (Wheat.Part enumPart in Enum.GetValues(typeof(Wheat.Part))){
                if (materialName.Contains(enumPart.ToString())){
                    return enumPart;
                }
            }
            return Wheat.Part.Wheat;
        }
    }

    public String age
    {
        get { return GetAgeFromMaterialName(materialName); }
    }

    public Material material
    {
        get { return gameObject.transform.GetComponent<Renderer>().material; }
        set { material = value; }
    }

    public String materialName;
    
    private void Start(){
        
    }


    // Set the material of this object to the material corresponding to its part in Wheat.cs
    public void ToggleAnnotationOn(Dictionary<Wheat.Part, Material> partMaterialDict){
        gameObject.transform.GetComponent<Renderer>().material = partMaterialDict.GetValueOrDefault(part, material);
    }

    // Set the material of this object back to the original material
    public void ToggleAnnotationOff(){
        gameObject.transform.GetComponent<Renderer>().material = material;
    }

    private string GetAgeFromMaterialName(String materialName){
        if (materialName.Contains("Young")){
            return "Young";
        } else if (materialName.Contains("Middle")){
            return "Middle";
        } else if (materialName.Contains("Mature")){
            return "Mature";
        }
        return "Unknown";
    }
}
