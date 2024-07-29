using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class TerrainHandler : MonoBehaviour {
    [SerializeField] private GameObject terrainObject;
    public float maxMove = 10f;
    private TerrainLayer[] activeTerrainLayers => terrainObject.GetComponent<Terrain>().terrainData.terrainLayers;
    [SerializeField] private List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
    

    public enum TerrainType {
        dimRedCoarse = 0,
        darkBrownMulch = 1,
        brightDryMud = 2,
        dimBrownGrass = 3,
        brightDryStraw = 4
    }

    private Vector3 initialPosition;
    void Start(){
        initialPosition = terrainObject.transform.position;
    }

    public void ResetTerrainPosition(){
        terrainObject.transform.position = initialPosition;
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