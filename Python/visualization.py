import json
import matplotlib.pyplot as plt
from pycocotools.coco import COCO
from PIL import Image
import numpy as np

# Load COCO JSON file
coco_json_path = "C:/Users/xSkul/OneDrive/Documents/Projects/Wheat/wheat/Datasets/1024x1024-test/domain1/updated_coco_standard1.json"
image_directory = "C:/Users/xSkul/OneDrive/Documents/Projects/Wheat/wheat/Datasets/1024x1024-test/domain1/"

with open(coco_json_path) as f:
    coco_data = json.load(f)

# Create a dictionary to map image IDs to file names and annotations
image_dict = {img['id']: img for img in coco_data['images']}
annotations_dict = {}
for annotation in coco_data['annotations']:
    img_id = annotation['image_id']
    if img_id not in annotations_dict:
        annotations_dict[img_id] = []
    annotations_dict[img_id].append(annotation)

# Function to draw polygons on images
def draw_polygons(image_path, annotations):
    image = Image.open(image_path)
    fig, ax = plt.subplots(1)
    ax.imshow(image)

    for annotation in annotations:
        for polygon in annotation['segmentation']:
            poly = np.array(polygon).reshape((len(polygon)//2, 2))
            polygon_patch = plt.Polygon(poly, linewidth=1, edgecolor='r', facecolor='none')
            ax.add_patch(polygon_patch)

    plt.show()

# Loop through images and their annotations
for img_id, image_info in image_dict.items():
    image_path = f"{image_directory}/{image_info['file_name']}"
    if img_id in annotations_dict:
        draw_polygons(image_path, annotations_dict[img_id])