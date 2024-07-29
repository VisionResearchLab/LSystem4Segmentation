using UnityEngine;
using System.Collections.Generic;

public class TerrainHandler : MonoBehaviour {
    [SerializeField] private GameObject terrainObject;
    public float maxMove = 10f;
    private TerrainLayer[] activeTerrainLayers => terrainObject.GetComponent<Terrain>().terrainData.terrainLayers;
    [SerializeField] private List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start(){
        initialPosition = terrainObject.transform.position;
        initialRotation = terrainObject.transform.rotation;
    }

    public void ResetTerrainPosition(){
        terrainObject.transform.SetPositionAndRotation(initialPosition, initialRotation);
    }

    public void MoveTerrainPosition(){
        Vector3 deltaPosition = new Vector3(Random.Range(-maxMove, maxMove), Random.Range(-maxMove, maxMove), Random.Range(-maxMove, maxMove));
        Vector3 newPosition = initialPosition + deltaPosition;
        terrainObject.transform.position = newPosition;
    }

    public void ChangeTexturesRandom(){
        // Create a new array of terrain layers
        TerrainLayer[] newActiveTerrainLayers = new TerrainLayer[activeTerrainLayers.Length];

        // Set each value of the new array to a random terrain layer in the list
        for(int i = 0; i < activeTerrainLayers.Length; i++){
            int choiceIndex = Random.Range(0, terrainLayers.Count);
            newActiveTerrainLayers[i] = terrainLayers[choiceIndex];
        }

        // Set the terrain layers of the terrain object to the new array.
        terrainObject.GetComponent<Terrain>().terrainData.terrainLayers = newActiveTerrainLayers;
    }
}