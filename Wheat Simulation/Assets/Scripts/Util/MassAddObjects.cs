using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MassAddObjects : MonoBehaviour
{
    // Mesh to prefab conversion details
    static private string meshPath = "Meshes/Ground Cover Models";
    static private string prefabPath = "Prefabs/Ground Cover Models";
    static private string prefabFullPath = "Assets/Resources/Prefabs/Ground Cover Models/";
    [SerializeField] private float scaleModifier;


    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // Boundaries of wheat growth
    public GameObject boundary1;
    public GameObject boundary2;

    // Range of coordinates on the x axis where wheat can appear
    private float xMin;
    private float xMax;

    // Range of coordinates on the y axis where wheat can appear
    private float yMin;
    private float yMax;
    [SerializeField] private float yDifferenceMin;
    [SerializeField] private float yDifferenceMax;

    // Range of coordinates on the z axis where wheat can appear
    private float zMin;
    private float zMax;
    
    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    // Convert all meshes to prefabs before first frame
    private void Start()
    {
        GameObject[] allModels = Resources.LoadAll<GameObject>(meshPath);
        DeleteAllFilesInDirectory(prefabFullPath);

        foreach (GameObject model in Resources.LoadAll<GameObject>(meshPath)){
            // Instantiate the model prefab
            GameObject instantiatedModel = Instantiate(model);

            // // Loop through each child object and add components
            // foreach (Transform childTransform in instantiatedModel.GetComponentsInChildren<Transform>(true))
            // {
            //     // Check if the child object is not the root object
            //     if (childTransform != instantiatedModel.transform)
            //     {

            //     }
            // }

            // Adjust scale
            instantiatedModel.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

            // Save the modified prefab
            PrefabUtility.SaveAsPrefabAsset(instantiatedModel, prefabFullPath + model.name + ".prefab");

            // Destroy the instantiated model
            Destroy(instantiatedModel);
        }

        // Get obj placement bounds
        defineBoundaries();
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


    // Finds the positions of the boundary transforms, and ensures that objects will only spawn within these boundaries.
    private void defineBoundaries()
    {
        Vector3 pos1 = boundary1.GetComponent<Transform>().position;
        Vector3 pos2 = boundary2.GetComponent<Transform>().position;

        xMin = pos1.x;
        yMin = pos1.y;
        zMin = pos1.z;

        xMax = pos2.x;
        yMax = pos2.y;
        zMax = pos2.z;
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiate()
    {
        // Get the amount of wheat to place from the user input
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);

        GameObject[] prefabs = Resources.LoadAll<GameObject>(prefabPath);
        int prefabsCount = prefabs.Length;

        for (int i = 0; i < quantityToPlace; i++){
            // Choose prefab
            int prefabIndex = Random.Range(0, prefabsCount-1);
            GameObject prefab = prefabs[prefabIndex];
            
            Vector3 position = GetPositionInBounds();
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, position, rotation, parentObject.transform);
        }
    }

    private Vector3 GetPositionInBounds(){
        // X and Z positions are always assumed to be valid, because ground exists everywhere
        float x = Random.Range(xMin, xMax);
        float y_increase = Random.Range(yDifferenceMin, yDifferenceMax); // How far above the raycast hit should it be placed?
        float z = Random.Range(zMin, zMax);

        // The Y position must be found by casting a ray downwards, in case the ground is not level
        Ray ray = new Ray(origin: new Vector3(x, Mathf.Max(yMin, yMax), z), direction: new Vector3(0, -1, 0));
        RaycastHit hit;

        // Should only interact with the ground layer
        if(Physics.Raycast(ray, out hit, Mathf.Abs(yMax - yMin), Wheat.groundLayerMask)){
            return hit.point + new Vector3(0f, y_increase, 0f);
        } 
        else {
            return new Vector3(0,0,0);
        }
    }
}
