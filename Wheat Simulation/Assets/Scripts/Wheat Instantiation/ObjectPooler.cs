using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    public int expectedWheatCount = 5000; // Default size for each pool

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        InitializePools();
    }

    void InitializePools()
    {
        GameObject[] wheatPrefabs = Wheat.GetAllWheatPrefabs();
        int poolSize = expectedWheatCount / wheatPrefabs.Length;
        foreach (GameObject prefab in wheatPrefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
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
