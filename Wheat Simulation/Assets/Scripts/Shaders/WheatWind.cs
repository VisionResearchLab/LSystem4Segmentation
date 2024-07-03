using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatWind : MonoBehaviour
{
    public WindZone windZone;
    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    void Start(){
        meshFilter = GetComponent<MeshFilter>();
        originalVertices = meshFilter.mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
    }

    void Update(){
        if (windZone){
            float windStrength = windZone.windMain;
            Vector3 windDirection = windZone.transform.forward;

            for (int i = 0; i < originalVertices.Length; i++){
                Vector3 vertex = originalVertices[i];
                float windEffect = Mathf.Sin(Time.time + Vector3.Dot(vertex, windDirection)) * windStrength;
                modifiedVertices[i] = vertex + new Vector3(0, windEffect, 0);
            }

            meshFilter.mesh.vertices = modifiedVertices;
            meshFilter.mesh.RecalculateNormals();
        }
    }

    public void UpdateWindZone(WindZone newWindZone){
        windZone = newWindZone;
    }
}
