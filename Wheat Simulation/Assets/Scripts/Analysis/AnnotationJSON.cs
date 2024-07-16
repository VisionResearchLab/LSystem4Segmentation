using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

[Serializable]
public class AnnotationJSON
{
    public List<Image> images = new List<Image>();
    public List<Annotation> annotations = new List<Annotation>();
    public List<Category> categories = new List<Category>();
}

[Serializable]
public class Image{
    public int id;
    public int width;
    public int height;
    public string file_name;
    public string date_captured;

    // constructor
    public Image(int id, int width, int height, string file_name, string date_captured){
        this.file_name = file_name;
        this.width = width;
        this.height = height;
        this.id = id;
        this.date_captured = date_captured;
    }
}

// Meant to represent leaf, wheat head, etc. as ints. Basically a dictionary.
[Serializable]
public class Category {
    public int id;
    public string name;

    // constructor
    public Category(int id, string name){
        this.id = id;
        this.name = name;
    }
}

[Serializable]
public class Annotation {
    public int image_id; // image id
    public int category_id; // part id
    public Tuple<int, int>[] segmentation; // list of segmented pixels (pixel as int array with 2 values)
    public int area; // number of segmented pixels
    public int[] bbox; // xmin, ymin, xmax, ymax
    
    // constructor
    public Annotation(int image_id, int category_id, List<int[]> pixels){
        this.image_id = image_id;
        this.category_id = category_id;

        // get segmentation (int[][]) from pixels (List<int[]>)
        segmentation = new Tuple<int, int>[pixels.Count];
        for (int i = 0; i < pixels.Count; i++){
            segmentation[i] = new Tuple<int, int>(pixels[i][0], pixels[i][1]);
        }

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
