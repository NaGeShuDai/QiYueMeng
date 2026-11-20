import bpy
s=bpy.context.scene
c=bpy.data.collections['09 Cutaway Shell'];c.hide_render=False;c.hide_viewport=False
s.camera=bpy.data.objects['CAM 02 - Player entry']
s.cycles.samples=16
s.render.resolution_x=1100;s.render.resolution_y=850
s.render.filepath='E:/ProgramFiles/_Project/Puzzle/bedroom_interior.png'
bpy.ops.render.render(write_still=True)
