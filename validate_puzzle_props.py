import bpy,json,os
from io_scene_fbx import parse_fbx
base='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/'
m=json.load(open(base+'asset_manifest.json',encoding='utf8'))
def embedded(e):
    out=[len(p) for p in e.props if isinstance(p,bytes)] if e.id==b'Content' else []
    for c in e.elems:out+=embedded(c)
    return out
reports=[]
for entry in m['assets']+m['items']+[{'fbx':'Exports/PuzzleProps_All.fbx','mesh_count':sum(a['mesh_count'] for a in m['assets'])}]:
    path=base+entry['fbx']
    tree,_=parse_fbx.parse(path)
    payload=embedded(tree)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=path)
    objs=list(bpy.context.scene.objects);meshes=[o for o in objs if o.type=='MESH']
    assert meshes,path
    assert all(o.type in {'EMPTY','MESH'} for o in objs),path
    assert all(o.data.uv_layers for o in meshes),path
    if 'mesh_count' in entry:assert len(meshes)==entry['mesh_count'],(path,len(meshes))
    if 'parts' in entry:
        for part in entry['parts']:
            matches=[o for o in objs if o.name==part['name'] or o.name.startswith(part['name']+'.')]
            assert matches,(path,part['name'])
            if part.get('interaction'):assert matches[0].get('interaction')==part['interaction']
    reports.append({'file':entry['fbx'],'meshes':len(meshes),'embedded_texture_bytes':payload,'uv_complete':True})
assert any(r['embedded_texture_bytes'] for r in reports)
state=[1,1,1,0,0,0]
for index in [0,1,3]:
    for j in [(index-1)%6,index,(index+1)%6]:state[j]^=1
assert state==[1]*6
json.dump({'status':'passed','fbx_files':reports,'moon_solution_verified':True},open(base+'validation_report.json','w',encoding='utf8'),ensure_ascii=False,indent=2)
print('VALIDATED',len(reports),'FBX files; complete UVs, hierarchy metadata, mesh counts; moon solution',flush=True)
