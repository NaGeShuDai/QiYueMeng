import bpy,math
from mathutils import Vector
s=bpy.context.scene
wood=bpy.data.materials['PP_AntiqueWalnut'];brass=bpy.data.materials['PP_AgedBrass']
def box(root,name,loc,size,ma=wood,par=None):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 bpy.data.collections[root.name].objects.link(o);o.parent=par if par else root;o.location=loc;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(ma)
 b=o.modifiers.new('Joinery','BEVEL');b.width=.003;b.segments=2;return o
def raise_bay(name,delta,drawername):
 root=bpy.data.objects[name]
 for o in list(root.children):
  if o.name.startswith(('Plaque','Engraving','Pickup','Reward_','Storage cabinet')) or o.name==drawername:continue
  o.location.z+=delta
 for o in root.children:
  if o.name.startswith(('Storage cabinet side','Storage cabinet back')):o.location.z+=delta/2;o.dimensions.z+=delta
 p=bpy.data.objects[drawername]
 for o in p.children:
  if o.name.endswith(('_Side','_End')) or '_Side.' in o.name or '_End.' in o.name:o.location.z+=delta/2;o.dimensions.z+=delta
  elif o.name.endswith('_Pull'):o.location.z+=delta/2
 p['extra_height_m']=delta
raise_bay('P06_MoonMechanism',.07,'MoonRewardDrawer')
raise_bay('P07_ObjectSequence',.06,'SequenceRewardDrawer')
# Hollow out the compass plate housing around its reward drawer.
root=bpy.data.objects['P03_CompassBoard'];old=bpy.data.objects.get('Compass mount');bpy.data.objects.remove(old,do_unlink=True)
box(root,'Compass base',(0,0,.006),(.64,.31,.012))
for x in [-.31,.31]:box(root,'Compass cabinet side',(x,0,.046),(.02,.31,.08))
box(root,'Compass cabinet back',(0,.145,.046),(.64,.02,.08))
for x in [-.265,.265]:box(root,'Compass front shoulder',(x,-.145,.046),(.11,.02,.08))
# Real interlocking notch on the moon installation piece.
piece=bpy.data.objects['Pickup_MoonDisc'];body=bpy.data.objects['Moon centerpiece'];notch=bpy.data.objects.get('Centerpiece notch')
if notch:bpy.data.objects.remove(notch,do_unlink=True)
# A keyed disk outline with a rectangular cut-in at north.
rad=.071;half=.011;cut=.055;alpha=math.asin(half/rad);verts2=[]
for j in range(65):
 a=math.pi/2+alpha+j*(2*math.pi-2*alpha)/64;verts2.append((rad*math.cos(a),rad*math.sin(a)))
verts2.extend([(half,cut),(-half,cut)])
n=len(verts2);verts=[(x,y,z) for z in [-.011,.011] for x,y in verts2]
faces=[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
me=bpy.data.meshes.new('Keyed moon disk');me.from_pydata(verts,[],faces);me.materials.append(wood);body.data=me
# Word box corner tabs should move with cover rather than clip when opened.
cover=bpy.data.objects['FinalBox_Lid']
for o in list(bpy.data.objects['P08_FinalWoodBox'].children):
 if o.name.startswith('Chest corner strap'):loc=o.location.copy();o.parent=cover;o.location=loc-cover.location
# Storage sockets describe local placements and reward identities.
rewards={'Reward_Compass':'missing_compass','Reward_LightBulb':'light_bulb','Reward_WordBox':'P05_WordBox','Reward_MoonDisc':'moon_disc','Reward_BlockMa':'wood_block_码','Reward_SmallBoat':'Boat','Reward_TrayHandle':'tray_handle','Reward_FinalBox':'P08_FinalWoodBox','Reward_ExitKey':'exit_key'}
for name,item in rewards.items():
 o=bpy.data.objects.get(name)
 if o:o['item_id']=item;o['interaction']='spawn_reward'
# Save viewport in solid material colors: quick to navigate, full shading available on demand.
for screen in bpy.data.screens:
 for a in screen.areas:
  if a.type=='VIEW_3D':a.spaces.active.shading.type='SOLID';a.spaces.active.shading.color_type='MATERIAL';a.spaces.active.region_3d.view_perspective='CAMERA'
for image in bpy.data.images:
 if image.source=='FILE' and image.has_data:image.pack()
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/Poetry_PuzzleProps.blend')
print('FINAL saved: hollow reward compartments, keyed moon part, packed textures')
