import bpy
import os

def export_wheats(unity_project_path, domain):
    base_export_path = f"{unity_project_path}/Assets/Resources/Meshes/Wheat Models/{domain}/"
    
    # Ensure the export path exists
    if not os.path.exists(base_export_path):
        os.makedirs(base_export_path)

    # Name of the parent collection
    parent_collection_name = "Wheat Plant Objects"

    # Find the parent collection
    parent_collection = bpy.data.collections.get(parent_collection_name)

    # Collect the nodes from the material nodes to reset them after exporting baked materials
    previous_color_nodes = None

    if parent_collection is None:
        print(f"Collection '{parent_collection_name}' not found!")
    else:
        # Auto bake (calls AutoBakeAllMaterials which calls several fn's from AutoBake)
        previous_color_nodes = AutoBakeAllMaterials(unity_project_path, domain)
        
        # Iterate through all child collections of the parent collection
        for collection in parent_collection.children:
            # Set the active collection
            bpy.context.view_layer.active_layer_collection = bpy.context.view_layer.layer_collection.children[parent_collection.name].children[collection.name]

            # Select all objects in the collection
            for obj in bpy.context.view_layer.objects:
                obj.select_set(obj.name in collection.objects)

            # Export the selected objects to the base export folder
            export_path = base_export_path + collection.name + ".fbx"
            bpy.ops.export_scene.fbx(filepath=export_path, use_selection=True, path_mode="COPY", embed_textures=True)

            # Deselect all objects for the next iteration
            bpy.ops.object.select_all(action='DESELECT')
        
        print("Export completed!")
        
        ResetNodeConnections(previous_color_nodes)

# Get a head object, then call on Autobake to use that object to bake the Head material texture.
# Returns all objects that need to have their nodes reset
def AutoBakeAllMaterials(unity_project_path, domain) -> list:
    # helper functions
    def simple_unwrap_uv(obj):
    # Ensure the object is valid
        if obj is None or obj.type != 'MESH':
            print("Invalid object. Must be a mesh.")
            return

        # Make the object active and selected
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)

        # Switch to Edit Mode
        bpy.ops.object.mode_set(mode='EDIT')
        
        # Select all geometry
        bpy.ops.mesh.select_all(action='SELECT')

        # Print the count of selected vertices
        selected_verts = [v for v in obj.data.vertices if v.select]
        print(f"Selected vertices count before unwrapping: {len(selected_verts)}")

        # Use Smart UV Project
        bpy.ops.uv.smart_project(island_margin=0.001)

        # Switch back to Object Mode
        bpy.ops.object.mode_set(mode='OBJECT')

        # Deselect the object
        obj.select_set(False)

    def flatten(xss):
        return [x for xs in xss for x in xs]
    
    wheat_plant_objects = bpy.data.collections.get("Wheat Plant Objects")
    ## Bake the Head material
    # Get a Head object in the scene
    first_wheat_parts = wheat_plant_objects.children[0].objects
    
    head = None
    for obj in first_wheat_parts:
        if obj.name.startswith("Head"):
            head = obj
            break
        
    awn = None
    for obj in first_wheat_parts:
        if obj.name.startswith("Awn"):
            awn = obj
            break
    
    # Using the material on the head, use Autobake to bake the texture and update the mat. nodes
    previous_head_color_node = None # Need to save this to reset it later
    if head is not None:
        previous_head_color_node = Bake([head.name], unity_project_path, domain + "_Head")
    
    
    # Using the material on the awn, use Autobake to bake the texture and update the mat. nodes
    previous_awn_color_node = None # Need to save this to reset it later
    if awn is not None:
        previous_awn_color_node = Bake([awn.name], unity_project_path, domain + "_Awn")

    # Using the material on the stem, use Autobake to bake the texture and update the mat. nodes
    stem_object_names = [obj.name for obj in flatten([child.objects for child in wheat_plant_objects.children]) if "Stem" in obj.name]
    previous_stem_color_node = None # Need to save this to reset it later

    stems = [bpy.data.objects[stem_object_name] for stem_object_name in stem_object_names]
    for stem in stems:
        simple_unwrap_uv(stem)
    previous_stem_color_node = Bake(stem_object_names, unity_project_path, domain + "_Stem")
    
    ## Bake each leaf. Requires each leaf to run through the bake function, since their textures are procedurally generated.
    leaf_object_names = [obj.name for obj in flatten([child.objects for child in wheat_plant_objects.children]) if "Leaf[" in obj.name]
    
    previous_leaf_color_nodes = list()
    
    for leaf_object_name in leaf_object_names:
        obj = bpy.data.objects[leaf_object_name]
        # Get UV map
        simple_unwrap_uv(obj) # try to unwrap the UV so that baked texture does not come out black

        # Create a new single-user material
        material = obj.active_material
        if material.users > 1:
            material = material.copy()
            obj.data.materials[0] = material

        # Bake the material
        leaf_texture_name = f"{domain}_{leaf_object_name}"
        previous_leaf_color_nodes.append(Bake([leaf_object_name], unity_project_path, leaf_texture_name))
    
    # export_wheats will call another function to reattach these nodes to the output after exporting
    # includes: one wheat head, one awns, one stem, and every leaf
    return flatten([[previous_head_color_node, previous_awn_color_node, previous_stem_color_node], previous_leaf_color_nodes])

# from Autobake script
def Bake(object_names, unity_project_path, baked_texture_name, width=512, height=512):
    bpy.context.scene.render.engine = 'CYCLES'
    bpy.context.scene.cycles.device = 'GPU'
    # Get base path for images to be saved to
    image_path = f"{unity_project_path}/Assets/Resources/Textures/Blender Export Wheat/"
    
    # Name the image that will be saved
    baked_image_name = baked_texture_name + ".png"

    # Select the first object in the object list
    first_obj = bpy.data.objects[object_names[0]]
    bpy.context.view_layer.objects.active = first_obj
    first_obj.select_set(True)

    # Get the material
    material = first_obj.active_material

    # Create a new image to bake to
    baked_image = bpy.data.images.new(baked_texture_name, width, height)

    # Find the existing Image Texture node from the material or create a new one if not found
    baked_image_node = None
    nodes = material.node_tree.nodes
    for node in nodes:
        if node.type == 'TEX_IMAGE':
            baked_image_node = node
            break
    if not baked_image_node:
        baked_image_node = nodes.new(type='ShaderNodeTexImage')
        baked_image_node.name = baked_texture_name  # Name the node "BakedTexture"
    baked_image_node.image = baked_image

    # Set the image texture node as active for baking
    material.node_tree.nodes.active = baked_image_node  # Set the active node for baking

    # Set bake settings
    bpy.context.scene.render.bake.use_selected_to_active = False
    bpy.context.scene.render.bake.use_pass_direct = False
    bpy.context.scene.render.bake.use_pass_indirect = False
    bpy.context.scene.render.bake.use_pass_color = True

    # Bake loop for all object names
    if len(object_names) > 1:
        for object_name in object_names:
            obj = bpy.data.objects[object_name]
            bpy.context.view_layer.objects.active = obj
            obj.select_set(True)
            
            # Determine whether the existing texture should be cleared or added to
            bpy.context.scene.render.bake.use_clear = object_name==object_names[0] # only true on the first bake iteration
            
            # Perform the bake
            bpy.ops.object.bake(type='DIFFUSE')
    else:
        bpy.context.view_layer.objects.active = first_obj
        first_obj.select_set(True)
        
        bpy.context.scene.render.bake.use_clear = True
        
        # Perform the bake
        bpy.ops.object.bake(type='DIFFUSE')

    # Save the baked image to specified path
    baked_image.filepath_raw = image_path + baked_image_name
    baked_image.file_format = 'PNG'
    baked_image.save()

    # Find the existing Principled BSDF node
    principled_bsdf = GetPBSDF(nodes)

    # If Principled BSDF node is not found, create a new one
    if not principled_bsdf:
        principled_bsdf = nodes.new(type='ShaderNodeBsdfPrincipled')
    
    # Save the node that is connected to PBSDF to re-attach it after exporting
    previous_color_node = GetNodeAttachedToPBSDF(principled_bsdf)

    # Connect the baked texture to the Base Color of the Principled BSDF node
    material.node_tree.links.new(baked_image_node.outputs['Color'], principled_bsdf.inputs['Base Color'])

    # Find the output node
    output_node = None
    for node in nodes:
        if node.type == 'OUTPUT_MATERIAL':
            output_node = node
            break

    # Connect the Principled BSDF node to the material output
    material.node_tree.links.new(principled_bsdf.outputs['BSDF'], output_node.inputs['Surface'])

    print("Baking complete and material updated with baked texture")
    
    return material, previous_color_node # Used for returning the matnodes to their original state after export


# Set the PBSDF of the stem material nodes to a color instead of using linked node
def PrepareStemMaterial():
    # Get material
    material_name = "Stem"
    material = bpy.data.materials.get(material_name)
    
    # Get nodes and links from material
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    
    # Find the Hue/Saturation/Value node connected to Principled BSDF
    principled_bsdf_node = GetPBSDF(nodes)
    if principled_bsdf_node is None:
        print("Principled BSDF node not found.")
        return
    
    hsv_node = GetNodeAttachedToPBSDF(principled_bsdf_node)
    if hsv_node is None:
        print("Hue/Saturation/Value node not found.")
        return
    
    # Disconnect Hue/Saturation/Value node from Principled BSDF (if connected)
    for link in links:
        if link.from_node == hsv_node and link.to_node == principled_bsdf_node:
            links.remove(link)
    
    # Get wheat color group
    wheat_color_group = None
    for node in nodes:
        print(node.type)
        if node.type == 'GROUP':
            wheat_color_group = node
            break
    
    # Get color ramp node in wheat color group
    color_ramp = None
    for node in wheat_color_group.node_tree.nodes:
        if node.type == 'VALTORGB':
            color_ramp = node.color_ramp
            break
    
    # Get the color output from the color ramp and its surrounding operations
    age = min(bpy.context.scene.frame_current / 1500.0, 1.0) # Hardcoded
    multi = wheat_color_group.inputs['Value Multi'].default_value
    
    final_multi = min((age+0.3)/1.1, 1.0) * multi
    
    color_output = color_ramp.evaluate(age)
    
    color_value_altered = (color_output[0]*final_multi, color_output[1]*final_multi, color_output[2]*final_multi, 1.0)
    
    # Set the color on the Principled BSDF to the color output of Hue/Saturation/Value node
    principled_bsdf_node.inputs['Base Color'].default_value = color_value_altered
    
    print("Prepared stem material for exporting")
    
    return material, hsv_node # Must be saved to reset the material nodes after exporting
    
    
# In a Material Nodes, get the node that outputs to the input of the given PBSDF
def GetNodeAttachedToPBSDF(principled_bsdf):
    node = None
    if principled_bsdf:
        # assuming principled BSDF only has one link
        for input_socket in principled_bsdf.inputs:
            if input_socket.is_linked:
                link = input_socket.links[0]  # Assuming only one link
                node = link.from_node
                break
    return node

def GetPBSDF(nodes):
    # Find the existing Principled BSDF node
    for node in nodes:
        if node.type == 'BSDF_PRINCIPLED':
            return node

# Input: (material, color node)
# Reconnects the color node to the principled BSDF of the material
# Call after the process of baking and exporting
def ResetNodeConnections(old_nodes):
    for material_node_tuple in old_nodes:
        # Get information from input
        material = material_node_tuple[0]
        color_node = material_node_tuple[1]
        
        # Get the principled_BSDF to connect to
        nodes = material.node_tree.nodes
        principled_bsdf = GetPBSDF(nodes)
        
        # Connect the node and principled_bsdf
        material.node_tree.links.new(color_node.outputs['Color'], principled_bsdf.inputs['Base Color'])
        
    
# UI

# Main UI panel
class WHEAT_PT_main_panel(bpy.types.Panel):
    bl_label = "Export Wheat Objects"
    bl_idname = "WHEAT_PT_export_wheat_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Wheat"
 
    def draw(self, context):
        layout = self.layout
        mytool = context.scene.my_tool
        
        box = layout.box()
        row = box.row()
        row.prop(mytool, "unity_project_path")
        row = box.row()
        row.prop(mytool, "domain")
        
        row = box.row()
        row.operator("wheat.export_wheat_objects")
 
 
# Operator for the separate wheat parts button in UI
class WHEAT_OT_export_wheat_objects(bpy.types.Operator):
    bl_label = "Export Wheat Objects"
    bl_idname = "wheat.export_wheat_objects"
    
    def execute(self, context):
        scene = context.scene
        mytool = scene.my_tool
        
        export_wheats(mytool.unity_project_path, mytool.domain)
        
        return {'FINISHED'}
 
 
# Handle registration of UI classes

classes = [WHEAT_PT_main_panel, WHEAT_OT_export_wheat_objects]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
 
def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)

if __name__ == "__main__":
    register()
