using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabsFromMeshes : MonoBehaviour
{
    static private string wheatMeshPath = "Meshes/Wheat Models";
    static private string wheatPrefabPath = "Assets/Resources/Prefabs/Wheat Models/";

    [SerializeField] private float scaleModifier;

    // [SerializeField] private GameObject objectPoolerGameObject;
    // private ObjectPooler objectPooler;

    private void Start()
    {
        GameObject[] allWheatModels = Resources.LoadAll<GameObject>(wheatMeshPath);
        DeleteAllFilesInDirectory(wheatPrefabPath);

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
                    MeshCollider mc = childTransform.gameObject.AddComponent<MeshCollider>();
                    childTransform.gameObject.layer = Wheat.wheatLayer;

                    // Assign it the suitable layer
                    foreach (string partName in Wheat.nameToPartDict.Keys){
                        if (childTransform.gameObject.name.Contains(partName)){
                            Wheat.Part part = Wheat.nameToPartDict[partName];
                            int layer = Wheat.partToLayerDict[part];
                            childTransform.gameObject.layer = layer;
                        }
                    }
                    
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
        
        // // Call on ObjectPooler to initialize pools, now that the prefabs exist
        // objectPooler = objectPoolerGameObject.GetComponent<ObjectPooler>();
        // objectPooler.InitializePools();
    }

    // Used to delete all the prefabs that do not have a corresponding mesh
    private static void DeleteAllFilesInDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                    // Debug.Log($"Deleted file: {file}");
                }
                catch (IOException ioExp)
                {
                    Debug.LogError($"Error deleting file: {file}, {ioExp.Message}");
                }
            }

            AssetDatabase.Refresh();
            // Debug.Log("All files in the directory have been deleted.");
        }
        else
        {
            // Debug.LogError("The specified directory does not exist.");
        }
    }

}
