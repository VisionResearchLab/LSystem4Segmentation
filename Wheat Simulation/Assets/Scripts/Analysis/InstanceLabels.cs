using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[Serializable]
public class InstanceLabels
{
    public List<Image> images;
    public List<Annotation> annotations;
    public List<Category> categories;
}

public class Image{
    public string file_name;
    public int height;
    public int width;
    public int id;
}

public class Annotation {
    public List<int[]> pixels = new List<int[]>(); // list of segmented pixels (pixel as int array with 2 values)
    int image_id; // image id
    int category_id; // part id
    public int area; // number of segmented pixels
    int[] bbox; // xmin, ymin, xmax, ymax
    
    public Annotation(List<int[]> pixels, int image_id, int category_id){
        this.pixels = pixels;
        this.image_id = image_id;
        this.category_id = category_id;

        // get area
        this.area = pixels.Count;

        // get bounding box
        HashSet<int> xValues = new HashSet<int>();
        HashSet<int> yValues = new HashSet<int>();
        foreach (int[] pixel in pixels){
            xValues.Add(pixel[0]);
            yValues.Add(pixel[1]);
        }
        this.bbox = new int[]{xValues.Min(), yValues.Min(), xValues.Max(), yValues.Max()};
    }
}

public class Category {
    public int id;
    public string name;
    public Category(int id, string name){
        this.id = id;
        this.name = name;
    }
}