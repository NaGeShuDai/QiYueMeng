import bpy,json
s=bpy.context.scene
print('FILE',bpy.data.filepath)
print('COLLECTIONS',[(c.name,len(c.objects),c.hide_render,c.hide_viewport) for c in s.collection.children])
print('CAMERAS',[(o.name,list(o.location)) for o in s.objects if o.type=='CAMERA'])
print('MATERIALS',[(m.name,tuple(m.diffuse_color)) for m in bpy.data.materials])
print('WINDOWS',[(o.name,list(o.location),list(o.dimensions)) for o in s.objects if any(x in o.name.lower() for x in ['window','wall','ceiling'])])
m=bpy.data.materials.new('TEMP_portal_probe');m.use_nodes=True
try:
    n=m.node_tree.nodes.new('ShaderNodeBsdfRayPortal');print('PORTAL',[(i.name,i.identifier) for i in n.inputs])
except Exception as e:print('PORTAL_UNAVAILABLE',str(e))
bpy.data.materials.remove(m)
