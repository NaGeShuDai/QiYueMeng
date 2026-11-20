import bpy, os, json
from mathutils import Vector
ROOT='E:/ProgramFiles/_Project/Puzzle/Unity_Export'
os.makedirs(ROOT+'/Textures',exist_ok=True)
s=bpy.context.scene
bpy.context.window.view_layer=s.view_layers['01 Portal Preview - Cycles']
for c in s.collection.children:c.hide_viewport=False
for o in s.objects:o.hide_set(False)
source=list(s.objects)
forest=set(bpy.data.collections['11 Forest - remote portal world'].objects)
room=[];trees=[];portal=[]
deps=bpy.context.evaluated_depsgraph_get()
dst=bpy.data.scenes.new('UNITY_EXPORT');dst.unit_settings.system='METRIC'
for original in source:
 if original.type not in {'MESH','FONT'} or original.hide_render:continue
 ev=original.evaluated_get(deps)
 me=bpy.data.meshes.new_from_object(ev,preserve_all_data_layers=True,depsgraph=deps)
 o=bpy.data.objects.new(original.name,me);dst.collection.objects.link(o);o.matrix_world=original.matrix_world.copy()
 for k in original.keys():
  if isinstance(original[k],(str,int,float)):o[k]=original[k]
 if original in forest:trees.append(o)
 elif original.name.startswith('WINDOW_PORTAL'):portal.append(o)
 else:room.append(o)
bpy.context.window.scene=dst
print('Copied evaluated mesh objects',len(room),len(trees),len(portal),flush=True)
# Copy materials so the original scene remains untouched. Remove portal conditions.
copies={}
for o in room+trees:
 for slot in o.material_slots:
  old=slot.material
  if not old:continue
  if old not in copies:
   m=old.copy();m.name='Export_'+old.name;copies[old]=m
   nt=m.node_tree;out=next(n for n in nt.nodes if n.type=='OUTPUT_MATERIAL');p=next((n for n in nt.nodes if n.type=='BSDF_PRINCIPLED'),None)
   if p:nt.links.new(p.outputs[0],out.inputs['Surface'])
  slot.material=copies[old]
# Pack one UV atlas across room parts without combining their export meshes.
bpy.ops.object.select_all(action='DESELECT')
for o in room:o.select_set(True)
bpy.context.view_layer.objects.active=room[0]
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=1.1519,island_margin=.003,area_weight=.2,correct_aspect=True,scale_to_bounds=True)
bpy.ops.uv.pack_islands(rotate=True,margin=.002)
bpy.ops.object.mode_set(mode='OBJECT')
print('Atlas UVs ready',flush=True)
img=bpy.data.images.new('Room_BaseColor',width=4096,height=4096,alpha=False)
img.colorspace_settings.name='sRGB'
room_mats={slot.material for o in room for slot in o.material_slots if slot.material}
# Bake exact procedural base color via emission (unlit), then restore PBR outputs.
restore=[]
for m in room_mats:
 nt=m.node_tree;p=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED');out=next(n for n in nt.nodes if n.type=='OUTPUT_MATERIAL')
 em=nt.nodes.new('ShaderNodeEmission')
 if p.inputs['Base Color'].is_linked:nt.links.new(p.inputs['Base Color'].links[0].from_socket,em.inputs['Color'])
 else:em.inputs['Color'].default_value=p.inputs['Base Color'].default_value
 nt.links.new(em.outputs[0],out.inputs['Surface'])
 t=nt.nodes.new('ShaderNodeTexImage');t.image=img;nt.nodes.active=t;restore.append((m,p,out,em))
# Single bake mesh shares the packed UVs; export parts stay separate.
bpy.ops.object.duplicate();bpy.ops.object.join();baker=bpy.context.object;baker.name='TEMP_BAKE'
dst.render.engine='CYCLES';dst.cycles.samples=1;dst.render.bake.margin=8
bpy.ops.object.bake(type='EMIT')
img.filepath_raw=ROOT+'/Textures/Room_BaseColor.png';img.file_format='PNG';img.save()
bpy.data.objects.remove(baker,do_unlink=True)
for m,p,out,em in restore:
 nt=m.node_tree;nt.links.new(p.outputs[0],out.inputs['Surface']);nt.nodes.remove(em)
 # Rebuild a simple export-friendly PBR node graph.
 rough=p.inputs['Roughness'].default_value;metal=p.inputs['Metallic'].default_value
 nt.nodes.clear();out=nt.nodes.new('ShaderNodeOutputMaterial');p=nt.nodes.new('ShaderNodeBsdfPrincipled');p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 t=nt.nodes.new('ShaderNodeTexImage');t.image=img;nt.links.new(t.outputs['Color'],p.inputs['Base Color']);nt.links.new(p.outputs[0],out.inputs['Surface'])
for o in trees:
 for slot in o.material_slots:
  m=slot.material
  if m in room_mats:continue
  nt=m.node_tree;p=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED')
  for link in list(p.inputs['Base Color'].links):nt.links.remove(link)
  p.inputs['Base Color'].default_value=m.diffuse_color
  out=next(n for n in nt.nodes if n.type=='OUTPUT_MATERIAL');nt.links.new(p.outputs[0],out.inputs['Surface'])
for o in portal:
 uv=o.data.uv_layers.new(name='UVMap') if not o.data.uv_layers else o.data.uv_layers.active
 for poly in o.data.polygons:
  for li in poly.loop_indices:
   co=o.data.vertices[o.data.loops[li].vertex_index].co
   uv.data[li].uv=((co.y-.46)/1.38,(co.z-1.055)/1.49)
 o.data.materials.clear();m=bpy.data.materials.new('Window_RenderTexture_ReplaceInUnity');m.use_nodes=True;m.diffuse_color=(.08,.13,.08,1);p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=m.diffuse_color;o.data.materials.append(m)
def root(name,objects):
 r=bpy.data.objects.new(name,None);dst.collection.objects.link(r)
 for o in objects:matrix=o.matrix_world.copy();o.parent=r;o.matrix_world=matrix
 return r
r=root('Room',room);f=root('Forest_Remote_80m',trees);w=root('Window_Portal',portal)
def export(filename,objects):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=ROOT+'/'+filename,use_selection=True,object_types={'MESH','EMPTY'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,mesh_smooth_type='FACE',use_custom_props=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
export('Poetry_Escape_Room_Complete.fbx',room+trees+portal+[r,f,w])
export('Poetry_Escape_Room_Interior.fbx',room+portal+[r,w])
export('Poetry_Forest.fbx',trees+[f])
report={'room_meshes':len(room),'forest_meshes':len(trees),'portal_meshes':len(portal),'vertices':sum(len(o.data.vertices) for o in room+trees+portal),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in room+trees+portal),'baked_basecolor':'Textures/Room_BaseColor.png','texture_resolution':4096,'fbx_files':{n:os.path.getsize(ROOT+'/'+n) for n in os.listdir(ROOT) if n.endswith('.fbx')}}
with open(ROOT+'/export_report.json','w',encoding='utf-8') as h:json.dump(report,h,indent=2)
print('EXPORT COMPLETE',json.dumps(report),flush=True)
