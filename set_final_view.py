import bpy
s=bpy.context.scene
c=bpy.data.collections['09 Cutaway Shell'];c.hide_render=False;c.hide_viewport=False
s['default_view']='Enclosed vintage room. Hide collection 09 and use CAM 01 for cutaway. Model-only layer active; switch to Portal Preview layer for Cycles viewport forest.'
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
print('Saved enclosed room; camera 04; modeling view excludes forest')
