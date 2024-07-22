import json
import cv2
import numpy as np
from collections import defaultdict
import os
import sys
from tqdm import tqdm
from imantics import Mask
import matplotlib.pyplot as plt

if sys.argv[1] is not None:
    parent_directory = sys.argv[1]

    # Load the existing JSON file
    json_file_path = os.path.join(parent_directory, 'annotations.json')
    with open(json_file_path, 'r') as f:
        coco_data = json.load(f)

    # Remove the "annotationMaps" section
    annotation_maps = coco_data.pop('annotationMaps', [])

    print(f"Total annotation maps: {len(annotation_maps)}")

    # Dictionary to hold segmentation data
    segmentations = defaultdict(list)

    # Process each annotation map with a progress bar
    for annotation_map in tqdm(annotation_maps, desc="Processing annotation maps"):
        image_id = annotation_map['image_id']
        annotation_file = annotation_map['file_name']

        # Construct the full path to the annotation PNG file
        annotation_file_path = os.path.join(parent_directory, annotation_file)

        # Read the annotation PNG file, collapse the four channels to a 32 bit integer
        annotation_img = cv2.imread(annotation_file_path, cv2.IMREAD_UNCHANGED)
        if annotation_img.shape[2] == 4:
            # Extract each channel
            r_channel = annotation_img[:, :, 0]  # Red channel
            g_channel = annotation_img[:, :, 1]  # Green channel
            b_channel = annotation_img[:, :, 2]  # Blue channel
            a_channel = annotation_img[:, :, 3]  # Alpha channel

            # Combine channels into a single 32-bit integer
            annotation_img = (r_channel.astype(np.uint32) << 24) | \
                             (g_channel.astype(np.uint32) << 16) | \
                             (b_channel.astype(np.uint32) << 8) | \
                             (a_channel.astype(np.uint32))
        print("Img:")
        print(np.unique(annotation_img))

        if annotation_img is None:
            print(f"Failed to load annotation image: {annotation_file_path}")
            continue

        # Convert the annotation image to unique object IDs
        unique_ids = np.unique(annotation_img)

        for obj_id in unique_ids:
            # Create a binary mask for the current object ID
            binary_mask = (annotation_img == obj_id).astype(np.uint8)

            polygons = Mask(binary_mask).polygons()
            print(binary_mask)
            print(polygons)

            for polygon in polygons:
                segmentations[image_id].append(polygon.tolist())

    # Update the annotations in the COCO data with the segmentation data
    for annotation in tqdm(coco_data['annotations'], desc="Updating Annotations"):
        image_id = annotation['image_id']
        if image_id in segmentations:
            annotation['segmentation'] = segmentations[image_id]

    # # Save the updated JSON file using orjson for efficient serialization
    # updated_json_file_path = os.path.join(parent_directory, 'updated_coco.json')
    # try:
    #     with open(updated_json_file_path, 'w') as f:
    #         f.write(orjson.dumps(coco_data, option=orjson.OPT_INDENT_2))
    #     print(f"Efficiently serialized JSON saved to {updated_json_file_path}")
    # except Exception as e:
    #     print(f"Error saving JSON file: {e}")

    # Test with standard JSON module
    standard_json_file_path = os.path.join(parent_directory, 'updated_coco_standard1.json')
    try:
        with open(standard_json_file_path, 'w') as f:
            json.dump(coco_data, f, indent=2)
        print(f"Standard JSON serialized and saved to {standard_json_file_path}")
    except Exception as e:
        print(f"Error saving JSON file with standard json module: {e}")
else:
    print("Error: Directory of dataset is required as an argument.")