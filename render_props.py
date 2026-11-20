import bpy,sys
from mathutils import Vector
base='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/'
s=bpy.context.scene
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
mode=args[0] if args else 'overview'
s.render.engine='CYCLES';s.cycles.samples=16;s.cycles.use_denoising=True
if mode=='overview':
 s.camera=bpy.data.objects['CAM_AssetOverview'];s.render.resolution_x=1800;s.render.resolution_y=1200;s.render.filepath=base+'Renders/PropLibrary.png';bpy.ops.render.render(write_still=True)
else:
 for name in mode.split(','):
  for c in s.collection.children:c.hide_render=c.name not in [name,'STUDIO_DoNotExport']
  s.camera=bpy.data.objects['CAM_'+name];s.render.resolution_x=1000;s.render.resolution_y=900;s.render.filepath=base+'Renders/'+name+'.png';bpy.ops.render.render(write_still=True)
