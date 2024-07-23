import json
import cv2
import numpy as np
from collections import defaultdict
import os
import orjson
from tqdm import tqdm
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon
from matplotlib.collections import PatchCollection

# Define the parent directory of your data
parent_directory = 'C:/Users/xSkul/OneDrive/Documents/Projects/Wheat/wheat/Datasets/1024x1024-8/domain2/'

# Load the existing JSON file
json_file_path = os.path.join(parent_directory, 'annotations.json')
with open(json_file_path, 'r') as f:
    coco_data = json.load(f)

# Remove the "annotationMaps" section
annotation_maps = coco_data.pop('annotationMaps', [])

print(f"Total annotation maps: {len(annotation_maps)}")

# Dictionary to hold segmentation data
segmentations = defaultdict(list)


def simplify_polygon(polygon, epsilon=0.01, precision=1):
    """Simplify a polygon using Douglas-Peucker algorithm and round coordinates."""
    points = np.array(polygon, dtype=np.float32).reshape(-1, 1, 2)
    if points.shape[0] < 6:
        return polygon
    epsilon *= cv2.arcLength(points, True)
    simplified_points = cv2.approxPolyDP(points, epsilon, True)
    simplified_polygon = np.round(simplified_points.flatten(), decimals=precision).tolist()
    return simplified_polygon


def plot_polygon_on_image(image_path, polygons, output_path=None):
    """Plot polygons on an image and save or show the result."""
    img = cv2.imread(image_path)
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    fig, ax = plt.subplots()
    ax.imshow(img_rgb)

    patches = []
    for poly in polygons:
        poly = np.array(poly).reshape(-1, 2)
        patch = Polygon(poly, closed=True, fill=None, edgecolor='r')
        patches.append(patch)

    p = PatchCollection(patches, match_original=True)
    ax.add_collection(p)
    ax.set_title(f"Polygons for {os.path.basename(image_path)}")
    plt.axis('off')

    if output_path:
        plt.savefig(output_path, bbox_inches='tight')
    else:
        plt.show()


# Process each annotation map with a progress bar
for annotation_map in tqdm(annotation_maps, desc="Processing Annotation Maps"):
    image_id = annotation_map['image_id']
    annotation_file = annotation_map['file_name']

    # Construct the full path to the annotation PNG file
    annotation_file_path = os.path.join(parent_directory, annotation_file)

    # Read the annotation PNG file
    annotation_img = cv2.imread(annotation_file_path, cv2.IMREAD_GRAYSCALE)

    if annotation_img is None:
        print(f"Failed to load annotation image: {annotation_file_path}")
        continue

    # Convert the annotation image to unique object IDs
    unique_ids = np.unique(annotation_img)

    for obj_id in unique_ids:
        if obj_id == 0:
            continue  # Skip background

        # Create a binary mask for the current object ID
        binary_mask = (annotation_img == obj_id).astype(np.uint8)

        # Find contours
        contours, _ = cv2.findContours(binary_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

        for contour in contours:
            if (len(contour) > 2):
                epsilon = 0.1 * cv2.arcLength(contour, True)  # Adjust epsilon as needed
                simplified_contour = cv2.approxPolyDP(contour, epsilon, True)

                # Convert contours to a polygon
                polygon = simplified_contour.flatten().tolist()
                optimized_polygon = simplify_polygon(polygon, epsilon=0.1)
                segmentations[image_id].append(optimized_polygon)

# Update the annotations in the COCO data with the segmentation data
for annotation in tqdm(coco_data['annotations'], desc="Updating Annotations"):
    image_id = annotation['image_id']
    if image_id in segmentations:
        annotation['segmentation'] = segmentations[image_id]

# Save the updated JSON file using orjson for efficient serialization
updated_json_file_path = os.path.join(parent_directory, 'updated_coco.json')
with open(updated_json_file_path, 'wb') as f:
    f.write(orjson.dumps(coco_data, option=orjson.OPT_INDENT_2))

print(f"Efficiently serialized JSON saved to {updated_json_file_path}")
