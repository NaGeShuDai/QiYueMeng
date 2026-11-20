import bpy,json,os
root='E:/ProgramFiles/_Project/Puzzle/Unity_Export'
from io_scene_fbx import parse_fbx
tree,version=parse_fbx.parse(root+'/Poetry_Escape_Room_Complete.fbx')
def contents(e):
 result=[]
 if e.id==b'Content':result.extend(len(p) for p in e.props if isinstance(p,bytes))
 for c in e.elems:result.extend(contents(c))
 return result
embedded=contents(tree)
assert any(n>1000000 for n in embedded),embedded
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=root+'/Poetry_Escape_Room_Complete.fbx')
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
images=[{'name':i.name,'width':i.size[0],'height':i.size[1],'has_pixels':i.has_data,'path':i.filepath} for i in bpy.data.images]
report={'reimported_meshes':len(meshes),'meshes_without_uv':[o.name for o in meshes if not o.data.uv_layers],'images':images,'root_groups':[o.name for o in bpy.context.scene.objects if o.parent is None],'materials':len(bpy.data.materials),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)}
assert len(meshes)==529,report
assert all(o.data.uv_layers for o in meshes if o.name.startswith('WINDOW_PORTAL'))
report['embedded_texture_bytes']=embedded
assert any(i['width']==4096 and i['has_pixels'] for i in images),report
with open(root+'/validation_report.json','w',encoding='utf8') as f:json.dump(report,f,indent=2)
print('VALIDATED',json.dumps(report),flush=True)
