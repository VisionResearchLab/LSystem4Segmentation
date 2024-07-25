using UnityEditor;
using UnityEngine;
using System.IO;

public class Domain {
    public string name;
    public PositionFinder.FieldLayout arrangement;
    public int wheatCount;
    public int underbrushCount;

    // Inputs
    public Domain(
        string name,
        PositionFinder.FieldLayout arrangement  = PositionFinder.FieldLayout.Uniform, 
        int wheatCount                          = 2000, 
        int underbrushCount                     = 20000)
        {
            this.name = name;
            this.arrangement = arrangement;
            this.wheatCount = wheatCount;
            this.underbrushCount = underbrushCount;
        }

    public void Build(){
        // Clear previous domain
        ObjectPooler.ClearAllPools();

        // Create the wheat prefabs pool for this domain
        string wheatPrefabsDirectory = GetPrefabDirectory("Wheat Models", name);
        ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Wheat, wheatPrefabsDirectory, wheatCount);

        // Create the underbrush prefabs pool for this domain
        string underbrushPrefabsDirectory = GetPrefabDirectory("Ground Cover Models", name);
        ObjectPooler.InitializePoolsFromDirectory(ObjectPooler.PoolType.Underbrush, underbrushPrefabsDirectory, underbrushCount);

        // Instantiate the wheat
        InstantiateWheat instantiateWheat = Object.FindObjectOfType<InstantiateWheat>();
        instantiateWheat.LoopAddWheat(wheatCount, arrangement);

        // Instantiate underbrush
        UnderbrushHandler underbrushHandler = Object.FindObjectOfType<UnderbrushHandler>();
        underbrushHandler.LoopInstantiateUnderbrushInBounds(underbrushCount, arrangement);
    }

    static private string GetPrefabDirectory(string prefabType, string domainName){
        string relativePath = $"Assets/Resources/Prefabs/{prefabType}/{domainName}";
        string fullPath = Path.GetFullPath(relativePath);
        if (Directory.Exists(fullPath))
            return fullPath;
        else {
            Debug.LogError("Could not find the domain path.");
            return null;
        }
    }
}