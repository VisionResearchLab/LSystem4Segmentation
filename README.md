# Wheat Head Segmentation Dataset Generator

## Description
This application is designed for the purpose of generating synthetic datasets for wheat head segmentation.

## Usage
### Generating models via Blender
In Blender, to generate a wheat model, follow these steps:
1. Go to the scripting section, and activate all scripts. These should open custom menus that control the wheat model generation. If you can't see the menus, mouse over a 3D viewport view panel, then press N on your keyboard and navigate to the "Wheat" section on the right side of this panel.
2. Adjust the parameters listed under Instantiate Wheat as desired, then click "Create Wheat". If you plan to export the wheat to Unity, leave the position and position variance at (0,0,0).
3. Click on the "Separate Wheat Parts" button to apply all modifiers to all wheat models, then separate them into folders that contain their parts. This makes the process of baking textures and exporting to Unity much easier.
4. Enter the Unity project directory into the field above the "Export Wheat Models" button, then click it to export the wheat models to Unity.
If you create other objects and want to export them to Unity as well, use the "Export Selected Objects" button.

### Creating dataset via Unity
In Unity, to generate a wheat model, follow these steps:
1. Enter Play mode.
2. Enter the number of objects that you wish to place into the bottom left input field.
3. Press a button on your keyboard according to the commands listed on screen to instantiate objects in the field in your desired arrangement.
4. Enter the directory where the dataset should be created.
5. Once you are ready to generate the dataset, press Y to start generation.
6. Once you are finished generating data, press U to stop generation, or simply exit play mode.
The dataset will consist of images and corresponding labels in the same folder. They have identical names of the format "YYYY-MM-DD_HH-mm-SS_image.png" for images and "YYYY-MM-DD_HH-mm-SS_label.png" for labels.

## Support
For help, contact Elijah Mickelson at the following email:
elijah.mickelson@ucalgary.ca

## Roadmap
Future goals:
- Simplify exporting of objects that are not single users of their materials
- Use geonodes to generate randomized leaf models
- Improve texture variety, especially for stem
- Implement a file reader to automatically progress through each stage of dataset generation

## Authors and acknowledgment
Developer: Elijah Mickelson
Special thanks to Hosein Beheshti and Dr. Farhad Maleki for the helpful advice in improving this program.

## Project status
The project will be frequently improved upon until September 2024.