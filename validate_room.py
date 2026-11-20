import bpy, json
s=bpy.context.scene
for other in list(bpy.data.scenes):
    if other != s and other.name.startswith('Poetry Room') and len(other.objects)==0:
        bpy.data.scenes.remove(other)
s.name='Poetry Room | Bedroom Escape'
for a in bpy.context.screen.areas:
    if a.type=='VIEW_3D':
        a.spaces.active.shading.type='MATERIAL'
        a.spaces.active.region_3d.view_perspective='CAMERA'
print(json.dumps({'scene':s.name,'objects':len(s.objects),'cameras':[o.name for o in s.objects if o.type=='CAMERA'],'puzzle_objects':[o.name for o in s.objects if 'puzzle_stage' in o],'shell_retained':len(bpy.data.collections['09 Cutaway Shell'].objects),'missing_external_images':[i.name for i in bpy.data.images if i.source=='FILE' and not i.has_data]}))
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
