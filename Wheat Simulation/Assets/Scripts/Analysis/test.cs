using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class test : MonoBehaviour {
    void Start(){
        List<int[]> pixelList = new List<int[]>
        {
            new int[] { 1, 1 },
            new int[] { 1, 2 },
            new int[] { 2, 1 },
            new int[] { 2, 2 },
            new int[] { 5, 5 },
            new int[] { 5, 6 },
            // Add more pixels as needed to create distinct clusters
        };
        int imageWidth = 10;
        int imageHeight = 10;

        List<List<int[]>> polygons = MaskToPolygons.GeneratePolygons(pixelList);
        Debug.Log(polygons);
        int polyCounter = 0;
        foreach (List<int[]> polygon in polygons){
            foreach(int[] vertex in polygon){
                Debug.Log($"Vertex of polygon {polyCounter} found at ({vertex[0]}, {vertex[1]}");
                polyCounter ++;
            }
        }
    }
}

