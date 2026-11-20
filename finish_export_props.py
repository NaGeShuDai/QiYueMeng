import bpy
count=0
for o in bpy.context.scene.objects:
    if o.type=='MESH' and not o.data.uv_layers:
        uv=o.data.uv_layers.new(name='UVMap')
        for poly in o.data.polygons:
            axis=max(range(3),key=lambda i:abs(poly.normal[i]))
            axes=[i for i in range(3) if i!=axis]
            for li in poly.loop_indices:
                co=o.data.vertices[o.data.loops[li].vertex_index].co
                uv.data[li].uv=(co[axes[0]]*4+.5,co[axes[1]]*4+.5)
        count+=1
bpy.ops.wm.save_as_mainfile(filepath=bpy.data.filepath)
print('UV_FIXED',count,flush=True)
exec(compile(open('E:/ProgramFiles/_Project/Puzzle/export_puzzle_props.py',encoding='utf8').read(),'export_puzzle_props.py','exec'))
