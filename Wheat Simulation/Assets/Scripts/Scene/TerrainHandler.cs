using UnityEngine;

public class TerrainHandler : MonoBehaviour {
    public GameObject terrainObject => GameObject.FindGameObjectWithTag("Ground");
    public float maxMove = 10f;
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
        terrainObject.transform.SetPositionAndRotation(newPosition, initialRotation);
    }
}