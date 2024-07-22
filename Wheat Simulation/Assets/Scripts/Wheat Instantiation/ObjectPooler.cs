using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    public int expectedWheatCount; // Default size for each pool
    public int expectedUnderbrushCount; // Default size for each pool

    void Start()
    {
        InitializePool(Wheat.GetAllWheatPrefabs(), expectedWheatCount);
        // InitializePool(UnderbrushHandler.GetAllUnderbrushPrefabs(), expectedUnderbrushCount);
    }

    public void InitializePool(GameObject[] allPrefabs, int expectedCount){
        int instancesPerObject = 25; // default
        if (allPrefabs.Length != 0){
            instancesPerObject = expectedCount / allPrefabs.Length;
        }

        foreach (GameObject prefab in allPrefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < instancesPerObject; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(prefab.name, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
