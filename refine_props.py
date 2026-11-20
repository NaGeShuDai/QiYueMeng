import bpy,math
from mathutils import Vector
s=bpy.context.scene
wood=bpy.data.materials['PP_AntiqueWalnut'];brass=bpy.data.materials['PP_AgedBrass'];dark=bpy.data.materials['PP_DarkWood']
def part(root,name,loc,size,ma=wood,par=None):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 bpy.data.collections[root.name].objects.link(o);o.parent=par if par else root;o.location=loc;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(ma);b=o.modifiers.new('Joinery bevel','BEVEL');b.width=.004;b.segments=2;o.modifiers.new('Normals','WEIGHTED_NORMAL');return o
def undercabinet(root,width,depth,height):
 # Station working surface is lifted; storage bay remains below it.
 for o in list(root.children):
  if o.name.startswith(('Plaque','Engraving','Pickup')):continue
  o.location.z+=height
 for x in [-width/2+.01,width/2-.01]:part(root,'Storage cabinet side',(x,0,height/2),(.02,depth,height))
 part(root,'Storage cabinet back',(0,depth/2-.01,height/2),(width,.02,height))
 part(root,'Storage cabinet base',(0,0,.008),(width,depth,.016))
def resize_drawer(root,name,width,depth,height):
 p=bpy.data.objects[name];p.location.z=.022
 for child in list(p.children):bpy.data.objects.remove(child,do_unlink=True)
 part(root,name+'_Bottom',(0,0,.008),(width,depth,.016),par=p)
 for x in [-width/2+.01,width/2-.01]:part(root,name+'_Side',(x,0,height/2),(.02,depth,height),par=p)
 for y in [-depth/2+.01,depth/2-.01]:part(root,name+'_End',(0,y,height/2),(width,.02,height),par=p)
 part(root,name+'_Pull',(0,-depth/2-.017,height/2),(.10,.025,.016),brass,p)
 p['usable_interior_m']='%.3f,%.3f,%.3f'%(width-.04,depth-.04,height-.016)
# Third compass fits inside the boathouse (0.152m diameter).
r=bpy.data.objects['P02_RiverCrossing']
for o in list(bpy.data.collections[r.name].objects):
 if o.name.startswith('Reward boathouse') or o.name=='Boathouse roof':bpy.data.objects.remove(o,do_unlink=True)
for x in [.17,.37]:part(r,'Boathouse rebuilt side',(x,-.13,.235),(.018,.22,.19),dark)
part(r,'Boathouse rebuilt back',(.27,-.029,.235),(.20,.018,.19),dark)
part(r,'Boathouse rebuilt floor',(.27,-.13,.148),(.22,.22,.016),wood)
part(r,'Boathouse rebuilt roof',(.27,-.13,.338),(.25,.25,.025),wood)
p=bpy.data.objects['Boathouse_Door'];p.location=(.16,-.25,.24)
o=bpy.data.objects['Boathouse door'];o.location=(.11,0,0);o.dimensions=(.22,.018,.19)
bpy.data.objects['Reward_Compass'].location=(.27,-.13,.17)
# Lamp must actually contain the 0.49 x 0.29 x 0.22m word box.
r=bpy.data.objects['P04_WiringLamp'];undercabinet(r,.60,.55,.27);resize_drawer(r,'LampRewardDrawer',.56,.49,.24)
bpy.data.objects['Reward_WordBox'].location=(0,0,.05)
# Make the metal shade a hollow frustum, no opaque bottom cap.
o=bpy.data.objects['Brass shade'];verts=[];faces=[];n=48
for radius,z in [(.14,-.08),(.045,.08),(.041,.078),(.136,-.08)]:
 for i in range(n):a=i*2*math.pi/n;verts.append((radius*math.cos(a),radius*math.sin(a),z))
for ring in range(4):
 for i in range(n):faces.append((ring*n+i,ring*n+(i+1)%n,((ring+1)%4)*n+(i+1)%n,((ring+1)%4)*n+i))
mesh=bpy.data.meshes.new('Hollow lamp shade');mesh.from_pydata(verts,[],faces);mesh.materials.append(brass);o.data=mesh
# Correct the ring puzzle to a guaranteed solvable authored start.
for i in range(6):
 o=bpy.data.objects['MoonTile_%d'%i];o.rotation_euler.x=0 if i<3 else math.pi;o['initial_bright']=i<3;o['solution_note']='Click 1,2,4 (one-based); each flips self and two neighbors'
r=bpy.data.objects['P06_MoonMechanism'];r['initial_state']='111000';r['test_solution']='1,2,4';undercabinet(r,.43,.43,.11);resize_drawer(r,'MoonRewardDrawer',.38,.38,.10)
bpy.data.objects['Reward_SmallBoat'].location=(-.07,0,.04);bpy.data.objects['Reward_TrayHandle'].location=(.09,0,.04)
# Deep sorting cabinet can hold the final 0.47 x 0.32 x 0.20m box.
r=bpy.data.objects['P07_ObjectSequence'];undercabinet(r,.70,.43,.23);resize_drawer(r,'SequenceRewardDrawer',.65,.40,.21)
bpy.data.objects['Reward_FinalBox'].location=(0,0,.05)
# Provide mounting anchors / movable subparts metadata.
for rname in ['P01_NumberLock','P02_RiverCrossing','P03_CompassBoard','P04_WiringLamp','P05_WordBox','P06_MoonMechanism','P07_ObjectSequence','P08_FinalWoodBox']:
 root=bpy.data.objects[rname];root['reward_required_for_next']=True
 for o in bpy.data.collections[rname].objects:
  if ' · ' in o.name:o['display_only']=True
# Put final slot pads under the movable slot pivots so press animation moves both.
cover=bpy.data.objects['FinalBox_Lid']
pads=[o for o in list(cover.children) if o.name.startswith('WoodBlock socket')]
for i,pad in enumerate(sorted(pads,key=lambda o:o.location.x)):
 slot=bpy.data.objects['LetterSocket_%d'%i];loc=pad.location.copy();pad.parent=slot;pad.location=loc-slot.location
# Reposition cameras to fit taller lamp and deep storage units.
for name in ['P04_WiringLamp','P06_MoonMechanism','P07_ObjectSequence']:
 cam=bpy.data.objects['CAM_'+name];root=bpy.data.objects[name];target=root.location+Vector((0,0,.27));cam.location=root.location+Vector((.9,-1.25,1.65));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=1.40
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/Poetry_PuzzleProps.blend')
print('Refined reward storage fits, hollow shade, guaranteed-solvable moon start, movable slot pads')
