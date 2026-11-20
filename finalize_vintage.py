import bpy
s=bpy.context.scene
o=bpy.data.objects.get('Cool window light')
if o:o.hide_render=True;o.hide_set(True)
# Keep the editing view strictly room-only. Enabled render layer still includes the portal world.
bpy.context.window.view_layer=s.view_layers['02 Modeling - room only']
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
print('Saved: room-only modeling view active; portal preview render layer retained; harsh window fill removed')
