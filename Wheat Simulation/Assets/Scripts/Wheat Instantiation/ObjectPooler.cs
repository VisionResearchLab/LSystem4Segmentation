using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    public enum PoolType {
        Wheat = 0,
        Underbrush = 1
    }
    
    private Dictionary<PoolType, List<Queue<GameObject>>> poolsOfTypeDict = new Dictionary<PoolType, List<Queue<GameObject>>>();

    public int expectedWheatCount; // Default size for each pool
    public int expectedUnderbrushCount; // Default size for each pool

    void Start()
    {
        InitializePool(PoolType.Wheat, Wheat.GetAllWheatPrefabs(), expectedWheatCount);
        InitializePool(PoolType.Underbrush, UnderbrushHandler.GetAllUnderbrushPrefabs(), expectedUnderbrushCount);
    }

    public void InitializePool(PoolType poolType, GameObject[] poolPrefabs, int expectedInstanceCount){
        List<Queue<GameObject>> pools = new List<Queue<GameObject>>();
        poolsOfTypeDict[poolType] = pools;

        int instancesPerObject = 25; // default
        if (poolPrefabs.Length != 0){
            instancesPerObject = expectedInstanceCount / poolPrefabs.Length;
        }

        foreach (GameObject prefab in poolPrefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < instancesPerObject; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            pools.Add(objectPool);
        }
    }

    public GameObject SpawnFromPoolOfType(PoolType poolType, Vector3 position, Quaternion rotation)
    {
        List<Queue<GameObject>> pools = poolsOfTypeDict[poolType];
        Queue<GameObject> pool = pools[UnityEngine.Random.Range(0, pools.Count-1)];
        GameObject objectToSpawn = pool.Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        pool.Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
