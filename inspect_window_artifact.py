import bpy
from mathutils import Vector
s=bpy.context.scene;cam=bpy.data.objects['CAM 04 - Vintage window view'];deps=bpy.context.evaluated_depsgraph_get();frame=cam.data.view_frame(scene=s)
print('FRAME',[tuple(v) for v in frame])
for u,v in [(490/960,1-260/720),(500/960,1-304/720),(490/960,1-280/720)]:
 x=min(p.x for p in frame)+u*(max(p.x for p in frame)-min(p.x for p in frame));y=min(p.y for p in frame)+v*(max(p.y for p in frame)-min(p.y for p in frame))
 ray=(cam.matrix_world.to_3x3()@Vector((x,y,frame[0].z))).normalized();hit,loc,n,idx,o,mat=s.ray_cast(deps,cam.location,ray);print('HIT',u,v,o.name if o else None,tuple(loc))
