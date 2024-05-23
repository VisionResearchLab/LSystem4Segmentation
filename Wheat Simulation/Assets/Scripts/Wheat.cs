using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheat : MonoBehaviour
{
    // All parts of a wheat object are considered to have the Wheat type, as well as their own (i.e. Head, Stem, or Leaf) type.
    public enum Part {
        Wheat,
        Head,
        Stem,
        Leaf
    }

    // Maps each Part to a color material for annotating data
    public static Dictionary<Part, Material> partAnnotationMaterials = new Dictionary<Part, Material>();


    // Check if an obj is a wheat. If there is a second argument, also check if the obj is of the same part as the parameter.
    public static bool IsWheat(GameObject obj, Part part = Part.Wheat){
        WheatData wheatData = obj.GetComponent<WheatData>();
        if (wheatData){
            // If the user doesn't specify the part, but the object has a wheatData script, it is a wheat
            if (part == Part.Wheat){
                return true;
            }

            // If the user does specify the part, the wheatData.part must be the same, or return false
            if (part.Equals(wheatData.part)){
                return true;
            }
        } 
        return false;
    }


    // Add the part-material pairs to the dictionary
    private void Start(){
        partAnnotationMaterials.Add(Part.Head, Resources.Load("/Materials/RED", typeof(Material)) as Material);
        partAnnotationMaterials.Add(Part.Stem, Resources.Load("/Materials/BLUE", typeof(Material)) as Material);
        partAnnotationMaterials.Add(Part.Leaf, Resources.Load("/Materials/GREEN", typeof(Material)) as Material);
    }


    // Returns all Wheat objects in the scene
    public static HashSet<GameObject> getAllWheats(){
        HashSet<GameObject> wheats = new HashSet<GameObject>();
        foreach (GameObject obj in FindObjectsOfType<GameObject>()){
            if (IsWheat(obj)){
                wheats.Add(obj);
            }
        }
        return wheats;
    }

    
    // Swap materials of each wheat with simple colors, for annotation purposes
    public static void ToggleAnnotationOn(){
        foreach (GameObject wheat in getAllWheats()){
            WheatData wheatData = wheat.GetComponent<WheatData>();
            wheatData.ToggleAnnotationOn(partAnnotationMaterials);
        }
    }


    // Reset the materials of each wheat
    public static void ToggleAnnotationff(){
        foreach (GameObject wheat in getAllWheats()){
            WheatData wheatData = wheat.GetComponent<WheatData>();
            wheatData.ToggleAnnotationOff();
        }
    }
}
