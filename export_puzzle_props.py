import bpy,os,json,math
from mathutils import Vector
base='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/'
os.makedirs(base+'Exports/Items',exist_ok=True)
src=bpy.context.scene;deps=bpy.context.evaluated_depsgraph_get()
dst=bpy.data.scenes.new('Export_PuzzleProps');dst.unit_settings.system='METRIC'
mapping={};roots=[bpy.data.objects[name] for name in src['asset_roots'].split('|')]
def descendants(o):
 result=[o]
 for child in o.children:result.extend(descendants(child))
 return result
source=[o for r in roots for o in descendants(r) if not o.get('display_only',False)]
for old in source:
 if old.type in {'MESH','FONT','CURVE'}:
  me=bpy.data.meshes.new_from_object(old.evaluated_get(deps),preserve_all_data_layers=True,depsgraph=deps);o=bpy.data.objects.new(old.name,me)
 elif old.type=='EMPTY':o=bpy.data.objects.new(old.name,None)
 else:continue
 dst.collection.objects.link(o);mapping[old]=o
 for k in old.keys():
  val=old[k]
  if isinstance(val,(str,int,float,bool)):o[k]=val
for old,o in mapping.items():
 if old.parent in mapping:o.parent=mapping[old.parent];o.matrix_parent_inverse=old.matrix_parent_inverse.copy();o.matrix_basis=old.matrix_basis.copy()
 else:o.matrix_world=old.matrix_world.copy()
bpy.context.window.scene=dst
manifest={'units':'meters','up_axis':'Y (FBX)','forward_axis':'-Z (FBX)','assets':[],'items':[],'moon_initial':'111000','moon_solution_one_based':[1,2,4]}
def export(objects,path):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'EMPTY','MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_mesh_modifiers=False,use_custom_props=True,add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True)
def info(old):
 d={'name':old.name,'parent':old.parent.name if old.parent else None,'local_position':list(old.location),'local_rotation_degrees':[math.degrees(v) for v in old.rotation_euler]}
 for k in old.keys():
  if isinstance(old[k],(str,int,float,bool)):d[k]=old[k]
 return d
for root in roots:
 new=mapping[root];loc=new.location.copy();new.location=(0,0,0)
 objects=descendants(new)
 path=base+'Exports/'+root.name+'.fbx';export(objects,path)
 manifest['assets'].append({'id':root.name,'fbx':'Exports/'+root.name+'.fbx','mesh_count':sum(o.type=='MESH' for o in objects),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects if o.type=='MESH'),'parts':[info(o) for o in descendants(root) if o.type=='EMPTY']})
 new.location=loc
# Independent pickups, each at origin and using its own identity rotation.
pickups=[o for o in source if o.type=='EMPTY' and (o.get('interaction')=='pickup' or o.name.startswith('Token_'))]
for old in pickups:
 o=mapping[old];p=o.parent;mat=o.matrix_basis.copy();o.parent=None;o.location=(0,0,0);o.rotation_euler=(0,0,0)
 path=base+'Exports/Items/'+old.name+'.fbx';export(descendants(o),path)
 manifest['items'].append({'name':old.name,'item_id':old.get('item_id',old.name),'fbx':'Exports/Items/'+old.name+'.fbx'})
 o.parent=p;o.matrix_basis=mat
export(list(mapping.values()),base+'Exports/PuzzleProps_All.fbx')
with open(base+'asset_manifest.json','w',encoding='utf8') as f:json.dump(manifest,f,ensure_ascii=False,indent=2)
assert len(roots)==9
print('EXPORT_COMPLETE',len(roots),'groups,',len(pickups),'pickups,',sum(o.type=='MESH' for o in mapping.values()),'meshes',flush=True)
