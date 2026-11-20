import bpy
s=bpy.context.scene
s.cycles.transparent_max_bounces=512
print('Transparent bounce budget:',s.cycles.transparent_max_bounces)
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
