import bpy,sys
s=bpy.context.scene
bpy.context.window.view_layer=s.view_layers['01 Portal Preview - Cycles']
shell=bpy.data.collections['09 Cutaway Shell'];shell.hide_render=False;shell.hide_viewport=False
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
mode=args[0] if args else 'preview'
if mode=='preview':
 s.camera=bpy.data.objects['CAM 04 - Vintage window view'];s.render.resolution_x=960;s.render.resolution_y=720;s.cycles.samples=16
 s.render.filepath='E:/ProgramFiles/_Project/Puzzle/vintage_window.png';bpy.ops.render.render(write_still=True)
elif mode=='parallax':
 s.render.resolution_x=600;s.render.resolution_y=600;s.cycles.samples=8
 for name,suffix in [('CAM 05 - Window parallax left','left'),('CAM 06 - Window parallax right','right')]:
  s.camera=bpy.data.objects[name];s.render.filepath='E:/ProgramFiles/_Project/Puzzle/window_parallax_'+suffix+'.png';bpy.ops.render.render(write_still=True)
elif mode=='isolation':
 from mathutils import Vector
 d=bpy.data.cameras.new('Isolation test');o=bpy.data.objects.new('Isolation test',d);s.collection.objects.link(o);o.location=(84,1,2);o.rotation_euler=(Vector((99,1,2))-o.location).to_track_quat('-Z','Y').to_euler();s.camera=o
 s.render.resolution_x=256;s.render.resolution_y=256;s.cycles.samples=4;s.render.filepath='E:/ProgramFiles/_Project/Puzzle/forest_direct_visibility_test.png';bpy.ops.render.render(write_still=True)
