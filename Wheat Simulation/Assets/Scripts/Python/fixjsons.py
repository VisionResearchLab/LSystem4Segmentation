import glob
import json
import sys

# Copy a JSON file to a new location. Categories can be excluded from the final result by passing a list of ints.
def add_is_crowd_and_fix_bbox(domain_path):
    json_path = f"{domain_path}coco_annotations_onlywheatheads.json"

    # Load the existing JSON file
    with open(json_path, 'r') as f:
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
    with open(json_path, 'w') as f:
        json.dump(data, f, indent=2)


if sys.argv[1] is not None:
    dataset_path = sys.argv[1]
    domain_paths = glob.glob(f"{dataset_path}/*/")
    print(f"Domain paths found: {domain_paths}")
    if len(domain_paths) > 0:
        for domain_path in domain_paths:
            # For this domain, add iscrowd, change the way bbox is represented
            add_is_crowd_and_fix_bbox(domain_path)
else:
    print("arg 1: dataset_path, arg 2: domain_name")
