using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Schema;

public class UnderbrushHandler : MonoBehaviour
{
    // Mesh to prefab conversion details
    static private string meshPath = "Meshes/Ground Cover Models";
    static private string prefabPath = "Prefabs/Ground Cover Models";
    static private string prefabFullPath = "Assets/Resources/Prefabs/Ground Cover Models/";
    [SerializeField] private float scaleModifier;


    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // Boundaries of wheat growth
    public GameObject boundary0;
    public GameObject boundary1;

    // When placing underbrush, move it up or down by an amount within these values
    [SerializeField] private float yDifferenceMin;
    [SerializeField] private float yDifferenceMax;
    
    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    // Convert all meshes to prefabs before first frame
    private void Start()
    {
        GameObject[] allModels = Resources.LoadAll<GameObject>(meshPath);
        DeleteAllFilesInDirectory(prefabFullPath);

        // Make prefabs
        foreach (GameObject model in allModels){
            // Instantiate the model prefab
            GameObject instantiatedModel = Instantiate(model);

            // Adjust scale
            instantiatedModel.transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

            // Save the modified prefab
            PrefabUtility.SaveAsPrefabAsset(instantiatedModel, prefabFullPath + model.name + ".prefab");

            // Destroy the instantiated model
            Destroy(instantiatedModel);
        }
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
                }
                catch (IOException ioExp)
                {
                    Debug.LogError($"Error deleting file: {file}, {ioExp.Message}");
                }
            }

            AssetDatabase.Refresh();
        }
    }

    private GameObject getRandomPrefab(){
        GameObject[] prefabs = Resources.LoadAll<GameObject>(prefabPath);
        int prefabsCount = prefabs.Length;
        int prefabIndex = Random.Range(0, prefabsCount-1);
        return prefabs[prefabIndex];
    }

    // NOTE: in this function, the y transformation is applied
    public void InstantiateUnderbrush(Vector3 approximatePosition, Quaternion rotation){
        GameObject prefab = getRandomPrefab();

        float maxRaycastDistance = 2f;
        Vector3 position = GetGroundBelowPosition(approximatePosition, maxRaycastDistance);
        position += new Vector3(0f, Random.Range(yDifferenceMin, yDifferenceMax), 0f);

        Instantiate(prefab, position, rotation, parentObject.transform);
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiateUnderbrushInBounds()
    {
        // Get the amount of wheat to place from the user input
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);

        for (int i = 0; i < quantityToPlace; i++){
            Vector3 position = GetPositionInBounds();
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            InstantiateUnderbrush(position, rotation);
        }
    }

    private Vector3 GetPositionInBounds(){
        Vector3 bound0 = boundary0.transform.position;
        Vector3 bound1 = boundary1.transform.position;
        
        // X and Z positions are always assumed to be valid, because ground exists everywhere
        float x = Random.Range(bound0.x, bound1.x);
        float z = Random.Range(bound0.z, bound1.z);
        float y_highest = Mathf.Max(bound0.y, bound1.y);
        float y_difference = Mathf.Abs(bound0.y - bound1.y);
        Vector3 originalPosition = new Vector3(x, y_highest, z);

        return GetGroundBelowPosition(originalPosition, y_difference);
    }

    private Vector3 GetGroundBelowPosition(Vector3 originalPosition, float maxRaycastDistance){
        // The Y position must be found by casting a ray downwards, in case the ground is not level
        Ray ray = new Ray(origin: originalPosition, direction: Vector3.down);
        RaycastHit hit;

        // Should only interact with the ground layer
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, Wheat.groundLayerMask)){
            return hit.point;
        } 
        else {
            return new Vector3(0,0,0);
        }
    }
}
