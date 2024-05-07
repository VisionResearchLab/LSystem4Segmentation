using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Presets;
using UnityEngine;

public class Demo : MonoBehaviour
{
    // We start with an axiom, which is like the "seed" of the plant
    private string axiom = "abc";
    private string current;

    // A dictionary stores the rules that are applied to the axiom in each iteration
    private Dictionary<string, string> presets = new Dictionary<string, string>();

    void Awake(){
        // Set the current String to the axiom
        current = string.Copy(axiom);

        // Add prefixes
        AddPreset("a", "dbca");

        //TODO: REMOVE THIS LATER
        Debug.Log(current);
        for (int i = 0; i < 5; i++){
            ApplyRules();
        }
    }

    // When ApplyRules is called, each character in the current string is replaced according to a preset
    void ApplyRules(){
        StringBuilder newString = new StringBuilder();

        foreach (char character in current){
            string charAsString = character.ToString();
            if (presets.ContainsKey(charAsString)){
                newString.Append(presets[charAsString]);
            } else {
                newString.Append(charAsString);
            }
        }

        current = newString.ToString();
        Debug.Log(current);
    }

    void AddPreset(string axiom, string rule){
        presets.Add(axiom, rule);
    }    
}
