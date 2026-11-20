import bpy
s=bpy.context.scene
for c in list(bpy.data.collections):
    if len(c.objects)==0 and len(c.children)==0 and c.name[:2] in ['01','02','03','04','05','06','07','08','09']:
        bpy.data.collections.remove(c)
for c in s.collection.children:
    if c.name.endswith('.001'):c.name=c.name[:-4]
print([(c.name,len(c.objects)) for c in s.collection.children])
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
