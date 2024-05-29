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

    public Color color;
    
    private void Start(){
        originalMaterial = gameObject.transform.GetComponent<Renderer>().material;

        if (Wheat.wheatIsAnnotated){
            ToggleAnnotationOn();
        }

        defineSelfColor();
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

    private void defineSelfColor(){
        float red = 0f;
        float blue = 0f;
        float green = 0f;

        float maturityModifier = 0f;
        if (age == "Middle"){
            maturityModifier = 0.1f;
        }
        else if (age == "Mature"){
            maturityModifier = 0.2f;
        }

        if (part == Wheat.Part.Head){
            red += 0.8f + maturityModifier;
            green += UnityEngine.Random.Range(0f, 0.4f);
        }
        else if (part == Wheat.Part.Stem){
            blue += 0.8f + maturityModifier;
            red += UnityEngine.Random.Range(0f, 0.4f);
        } else if (part == Wheat.Part.Leaf){
            green += 0.8f + maturityModifier;
            blue += UnityEngine.Random.Range(0f, 0.4f);
        }

        color = new Color(red, green, blue);
    }
}
