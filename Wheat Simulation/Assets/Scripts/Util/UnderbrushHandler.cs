using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Schema;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

public class UnderbrushHandler : MonoBehaviour
{
    // HashSet of all underbrush objects in the scene
    private static HashSet<GameObject> underbrushObjects = new HashSet<GameObject>();


    // Mesh to prefab conversion details
    static private string meshPath = "Meshes/Ground Cover Models";
    static private string prefabPath = "Prefabs/Ground Cover Models";
    static private string prefabFullPath = "Assets/Resources/Prefabs/Ground Cover Models/";
    [SerializeField] private float scaleModifier;


    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // When placing underbrush, move it up or down by an amount within these values
    [SerializeField] private float yDifferenceMin;
    [SerializeField] private float yDifferenceMax;
    
    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    [SerializeField] private PositionFinder positionFinder;


    // Convert all meshes to prefabs before first frame
    void Start()
    {
        MeshesToPrefabs();
    }

    // Clear previously created prefabs and create new prefabs from meshes in assets/meshes/ground cover models
    private void MeshesToPrefabs(){
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

    // Get all prefabs
    public static GameObject[] GetAllUnderbrushPrefabs(){
        return Resources.LoadAll<GameObject>(prefabPath);
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
    public void InstantiateUnderbrush(Vector3 position, Quaternion rotation){
        GameObject prefab = getRandomPrefab();
        position += new Vector3(0f, Random.Range(yDifferenceMin, yDifferenceMax), 0f);
        // Instantiate a new object and add it to the hashset
        underbrushObjects.Add(Instantiate(prefab, position, rotation, parentObject.transform));
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiateUnderbrushInBounds(PositionFinder.FieldLayout shape = PositionFinder.FieldLayout.Uniform)
    {
        // Get the amount of wheat to place from the user input
        int quantityToPlace = int.Parse(quantityToPlaceInputField.GetComponent<TMPro.TMP_InputField>().text);

        for (int i = 0; i < quantityToPlace; i++){
            Vector3 position = positionFinder.GetPositionFromPattern(shape);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            InstantiateUnderbrush(position, rotation);
        }
    }

    public static HashSet<GameObject> getAllUnderbrush(){
        return underbrushObjects;
    }
}
