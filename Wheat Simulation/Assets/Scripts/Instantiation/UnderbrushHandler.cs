using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UnderbrushHandler : MonoBehaviour
{
    // HashSet of all underbrush objects in the scene
    private static HashSet<GameObject> underbrushObjects = new HashSet<GameObject>();


    // Mesh to prefab conversion details
    static private string meshPath = "Assets/Resources/Meshes/Ground Cover Models";
    static private string prefabPath = "Assets/Resources/Prefabs/Ground Cover Models/";
    [SerializeField] private float scaleModifier;


    // UI text field that sets the amount of wheat objects to place
    public TMPro.TMP_InputField quantityToPlaceInputField;

    // When placing underbrush, move it up or down by an amount within these values
    [SerializeField] private float yDifferenceMin;
    [SerializeField] private float yDifferenceMax;
    
    // Parent object to instantiate ground cover under
    [SerializeField] private GameObject parentObject;

    [SerializeField] private PositionFinder positionFinder;

    [SerializeField] private ObjectPooler objectPooler;

    // // Convert all meshes to prefabs before first frame
    // void Start()
    // {
    //     PrefabsFromMeshes.DeleteAllFilesAndDirectories(prefabPath);
        
    //     MeshesToPrefabs();
    // }

    // // Clear previously created prefabs and create new prefabs from meshes in assets/meshes/ground cover models
    // private void MeshesToPrefabs(){
    //     string[] meshDirectories = Directory.GetDirectories(meshPath);
    //     foreach (string meshDirectory in meshDirectories){
    //         string relativePath = meshDirectory.Split(new[] { "Assets/Resources/" }, System.StringSplitOptions.None)[1];
    //         GameObject[] allModels = Resources.LoadAll<GameObject>(relativePath);

    //         // Make prefabs
    //         foreach (GameObject model in allModels){
    //             // Instantiate the model prefab
    //             GameObject instantiatedModel = Instantiate(model);
    //             Transform transform = instantiatedModel.transform;

    //             // Adjust scale
    //             transform.localScale = new Vector3(scaleModifier, scaleModifier, scaleModifier);

    //             // If a tag is found 
    //             string name = transform.name;
    //             foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags){
    //                 if (name.ToLower().Contains(tag.ToLower())){
    //                     transform.tag = tag;
    //                 }
    //             }

    //             // Save the modified prefab
    //             string prefabDirectory = meshDirectory.Replace("Meshes", "Prefabs");
    //             PrefabUtility.SaveAsPrefabAsset(instantiatedModel, $"{prefabDirectory}/{model.name}.prefab");

    //             // Destroy the instantiated model
    //             Destroy(instantiatedModel);
    //         }
    //     }
        
    // }

    // NOTE: in this function, the y transformation is applied
    public void InstantiateUnderbrush(Vector3 position, Quaternion rotation){
        position += new Vector3(0f, Random.Range(yDifferenceMin, yDifferenceMax), 0f);

        // Instantiate a new object and add it to the hashset
        GameObject placedUnderbrush = ObjectPooler.SpawnFromPoolOfType(ObjectPooler.PoolType.Underbrush, position, rotation);
        placedUnderbrush.transform.SetParent(parentObject.transform);

        underbrushObjects.Add(placedUnderbrush);
    }

    // When called, create a new wheat object in a random position that is within the coordinates given above
    public void LoopInstantiateUnderbrushInBounds(int quantity, PositionFinder.FieldLayout shape = PositionFinder.FieldLayout.Uniform)
    {
        for (int i = 0; i < quantity; i++){
            Vector3 position = positionFinder.GetPositionFromPattern(shape);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            InstantiateUnderbrush(position, rotation);
        }
    }

    public static HashSet<GameObject> getAllUnderbrush(){
        return underbrushObjects;
    }
}
