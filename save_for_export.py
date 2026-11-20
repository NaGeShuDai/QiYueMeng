import bpy
bpy.ops.wm.save_as_mainfile(filepath=bpy.data.filepath)
print(bpy.data.filepath)
