import glob
import json
import shutil
import cv2
import numpy as np
from collections import defaultdict
import os
import sys
from tqdm import tqdm
from imantics import Mask


# Copy a JSON file to a new location. Categories can be excluded from the final result by passing a list of ints.
def add_is_crowd_and_fix_bbox(input_json_path):
    directory, file_name = os.path.split(input_json_path)
    output_json_path = f"{directory}/fixed_{file_name}"

    # Load the existing JSON file
    with open(input_json_path, 'r') as f:
        data = json.load(f)

    # Remove unused categories
    filtered_categories = [cat for cat in data['categories'] if (cat['name'] == "Ground" or cat['name'] == "Head")]
    data['categories'] = filtered_categories

    resolution = 1024
    # Fix the bounding box format and add iscrowd
    for annotation in data['annotations']:
        bounding_box = annotation['bbox']
        bl_x = bounding_box[0]
        bl_y = bounding_box[1]
        tr_x = bounding_box[2]
        tr_y = bounding_box[3]

        width = tr_x - bl_x
        height = tr_y - bl_y

        annotation['bbox'] = [bl_x, resolution - tr_y, width, height]
        annotation['iscrowd'] = 0

    # Write the new JSON file
    with open(output_json_path, 'w') as f:
        json.dump(data, f, indent=2)


if sys.argv[1] is not None:
    path = sys.argv[1]
    add_is_crowd_and_fix_bbox(path)
else:
    print("arg 1 should be json path")
