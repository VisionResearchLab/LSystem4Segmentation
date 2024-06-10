using System;
using UnityEditor;
using UnityEngine;

public class PrefabsFromMeshes : MonoBehaviour
{
    static private string wheatMeshPath = "Meshes/Wheat Models";
    static private string wheatPrefabPath = "Assets/Resources/Prefabs/Wheat Models/";

    [SerializeField] private float scaleModifier;

    // [SerializeField] private WindZone wheatWindZone;

    private void Start()
    {
        GameObject[] allWheatModels = Resources.LoadAll<GameObject>(wheatMeshPath);
        //if (!(allWheatModels.Length == Wheat.GetAllWheatPrefabs().Length)){
        foreach (GameObject wheatModel in Resources.LoadAll<GameObject>(wheatMeshPath)){
            // Instantiate the model prefab
            GameObject instantiatedModel = Instantiate(wheatModel);

            // Loop through each child object and add components
            foreach (Transform childTransform in instantiatedModel.GetComponentsInChildren<Transform>(true))
            {
                // Check if the child object is not the root object
                if (childTransform != instantiatedModel.transform)
                {
                    // Add components to the child object
                    childTransform.gameObject.AddComponent<WheatData>();
                    childTransform.gameObject.AddComponent<MeshCollider>();
                    childTransform.gameObject.layer = Wheat.wheatLayer;
                    
                    // // Add wind interaction script
                    // WheatWind wheatWind = childTransform.gameObject.AddComponent<WheatWind>();
                    // wheatWind.UpdateWindZone(wheatWindZone);
                }
            }

            // Set layer to "Wheat"
            instantiatedModel.layer = Wheat.wheatLayer;

            // Adjust scale
            instantiatedModel.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

            // Save the modified prefab
            PrefabUtility.SaveAsPrefabAsset(instantiatedModel, wheatPrefabPath + wheatModel.name + ".prefab");

            // Destroy the instantiated model
            Destroy(instantiatedModel);
        }
        // } else {
        //     Debug.Log("All models have already been converted to prefabs.");
        // }
    }
}
