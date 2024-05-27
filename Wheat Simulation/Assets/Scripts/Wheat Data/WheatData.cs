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

    public Material originalMaterial;

    public Material annotationMaterial {
        get { return Wheat.partAnnotationMaterials.GetValueOrDefault(part, originalMaterial); }
    }

    public String materialName {
        get { return originalMaterial.name; }
    }
    
    private void Start(){
        originalMaterial = gameObject.transform.GetComponent<Renderer>().material;

        if (Wheat.wheatIsAnnotated){
            ToggleAnnotationOn();
        }
    }


    // Set the material of this object to the material corresponding to its part in Wheat.cs
    public void ToggleAnnotationOn(){
        gameObject.transform.GetComponent<Renderer>().material = annotationMaterial;
    }

    // Set the material of this object back to the original material
    public void ToggleAnnotationOff(){
        gameObject.transform.GetComponent<Renderer>().material = originalMaterial;
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
