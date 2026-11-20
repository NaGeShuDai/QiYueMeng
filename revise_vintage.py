import bpy,math,random
from mathutils import Vector
random.seed(34)
s=bpy.context.scene
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room_before_vintage.blend')
def material(name,color,rough=.5):
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*color,1)
 p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
 return m
def wood(name,dark,light,scale):
 m=material(name,light);nt=m.node_tree;p=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED')
 tc=nt.nodes.new('ShaderNodeTexCoord');v=nt.nodes.new('ShaderNodeVectorMath');v.operation='MULTIPLY';v.inputs[1].default_value=scale;nt.links.new(tc.outputs['Generated'],v.inputs[0])
 n=nt.nodes.new('ShaderNodeTexNoise');n.inputs['Scale'].default_value=5;n.inputs['Detail'].default_value=3;n.inputs['Roughness'].default_value=.7;nt.links.new(v.outputs[0],n.inputs['Vector'])
 r=nt.nodes.new('ShaderNodeValToRGB');r.color_ramp.elements[0].color=(*dark,1);r.color_ramp.elements[1].color=(*light,1);nt.links.new(n.outputs['Fac'],r.inputs[0]);nt.links.new(r.outputs[0],p.inputs['Base Color'])
 bump=nt.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.16;bump.inputs['Distance'].default_value=.012;nt.links.new(n.outputs['Fac'],bump.inputs['Height']);nt.links.new(bump.outputs[0],p.inputs['Normal'])
 return m
wall=wood('V2 | smoked oak walls',(.045,.021,.009),(.21,.105,.045),(22,22,.7))
floor=wood('V2 | worn oak floor',(.055,.024,.009),(.27,.14,.055),(.7,30,4))
furniture=wood('V2 | antique walnut furniture',(.026,.011,.005),(.135,.057,.019),(18,18,.6))
trim=wood('V2 | dark carved walnut',(.018,.008,.004),(.075,.029,.009),(12,12,.6))
panels=wood('V2 | inset oak panels',(.065,.031,.012),(.25,.126,.048),(24,24,.5))
tan=material('V2 | natural flax bedding',(.30,.205,.115),.88)
brown=material('V2 | tobacco bedcover',(.12,.064,.026),.88)
brass=bpy.data.materials['Aged brass'];p=next(n for n in brass.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(.23,.12,.035,1);p.inputs['Roughness'].default_value=.42
# Preserve user-edited transforms; only change materials on existing objects.
for o in list(s.objects):
 if o.type not in {'MESH','FONT'}:continue
 for slot in o.material_slots:
  if not slot.material:continue
  name=slot.material.name
  if name.startswith('Honey oak'):slot.material=floor if 'floorboard' in o.name else furniture
  elif name=='Dark walnut':slot.material=trim
  elif name in ['Sage plaster','Ivory paint']:slot.material=wall
  elif name=='Linen':slot.material=tan
  elif name=='Petrol quilt':slot.material=brown
 if any(t in o.name for t in ['Pillow','Mattress']):o.data.materials.clear();o.data.materials.append(tan)
 if o.name=='Headboard upholstered panel':o.data.materials.clear();o.data.materials.append(panels);o.name='Headboard solid wood inset'
 if o.name=='Folded throw':o.data.materials.clear();o.data.materials.append(brown)
 if any(o.name.startswith(t) for t in ['Curtain fold','Curtain rod','Woven rug','Rug border']):o.hide_render=True;o.hide_set(True)
 if o.name=='Linen lamp shade':o.data.materials.clear();o.data.materials.append(brass)
current=bpy.data.collections.new('10 Vintage woodwork');s.collection.children.link(current)
def put(o,name,ma):
 o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 current.objects.link(o)
 if ma:o.data.materials.append(ma)
 return o
def box(name,loc,dims,ma,bevel=.01):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=put(bpy.context.object,name,ma);o.dimensions=dims;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  b=o.modifiers.new('Rounded joinery','BEVEL');b.width=bevel;b.segments=2;o.modifiers.new('Normals','WEIGHTED_NORMAL')
 return o
# Raised panelling with actual wooden rails, replacing painted walls.
for x in [-2.22+i*.55 for i in range(9)]:
 box('North oak panel',(x,2.477,1.80),(.50,.025,1.94),panels,.007)
 for dx in [-.253,.253]:box('North panel beading',(x+dx,2.448,1.80),(.018,.024,1.97),trim,.003)
 for z in [.83,2.77]:box('North panel bead rail',(x,2.448,z),(.52,.024,.018),trim,.003)
for y in [-2.22,-1.67,-1.12,-.57,-.02,2.23]:
 box('East oak panel',(2.477,y,1.80),(.025,.50,1.94),panels,.007)
 for dy in [-.253,.253]:box('East panel beading',(2.448,y+dy,1.80),(.024,.018,1.97),trim,.003)
for x in [-1.7,0,1.7]:box('Ceiling oak beam',(x,0,2.94),(.15,5,.18),trim,.02)
for y in [.23,2.07]:
 box('Window wooden shutter',(2.29,y,1.80),(.10,.30,1.52),furniture,.014)
 for z in [1.13+i*.11 for i in range(13)]:
  o=box('Shutter louver',(2.22,y,z),(.08,.24,.055),panels,.004);o.rotation_euler.y=.25
 for z in [1.17,2.40]:box('Shutter forged hinge',(2.16,y,z),(.014,.23,.026),brass,.003)
# Forest remote space. Its objects do not surround or obscure the room in modeling.
current=bpy.data.collections.new('11 Forest - remote portal world');s.collection.children.link(current)
offset=80.0
def portal_only(m):
 nt=m.node_tree;out=next(n for n in nt.nodes if n.type=='OUTPUT_MATERIAL');old=out.inputs['Surface'].links[0].from_socket
 lp=nt.nodes.new('ShaderNodeLightPath')
 if 'Portal Depth' not in lp.outputs:raise RuntimeError('Portal Depth is required')
 cond=nt.nodes.new('ShaderNodeMath');cond.operation='GREATER_THAN';cond.inputs[1].default_value=.5;nt.links.new(lp.outputs['Portal Depth'],cond.inputs[0])
 trans=nt.nodes.new('ShaderNodeBsdfTransparent');mix=nt.nodes.new('ShaderNodeMixShader');nt.links.new(cond.outputs[0],mix.inputs[0]);nt.links.new(trans.outputs[0],mix.inputs[1]);nt.links.new(old,mix.inputs[2]);nt.links.new(mix.outputs[0],out.inputs['Surface'])
 m['visibility']='Rendered only after passing a Ray Portal; use ordinary material in Unity'
bark=wood('FOREST | bark',(.018,.009,.004),(.10,.046,.016),(12,12,.5))
needles=[material('FOREST | foliage %d'%i,c,.9) for i,c in enumerate([(.045,.085,.025),(.075,.12,.035),(.11,.145,.047),(.025,.055,.022)])]
earth=material('FOREST | moss ground',(.075,.085,.029),.95)
stone=material('FOREST | weathered stones',(.16,.17,.125),.95)
sky=material('FOREST | pale mist backdrop',(.28,.34,.29),1)
sp=next(n for n in sky.node_tree.nodes if n.type=='BSDF_PRINCIPLED');sp.inputs['Emission Color'].default_value=(.27,.34,.29,1);sp.inputs['Emission Strength'].default_value=.6
for ma in [bark,*needles,earth,stone,sky]:portal_only(ma)
# Each tree is one mesh, keeping export and manipulation simple.
def tree_mesh(idx):
 verts=[];faces=[];ids=[]
 def cone(cx,cy,z,r0,r1,h,n,material_index):
  start=len(verts)
  for zz,rr in [(z,r0),(z+h,r1)]:
   for k in range(n):
    a=2*math.pi*k/n;verts.append((cx+math.cos(a)*rr,cy+math.sin(a)*rr,zz))
  faces.append(tuple(start+k for k in reversed(range(n))));ids.append(material_index)
  faces.append(tuple(start+n+k for k in range(n)));ids.append(material_index)
  for k in range(n):faces.append((start+k,start+(k+1)%n,start+n+(k+1)%n,start+n+k));ids.append(material_index)
 cone(0,0,0,.15,.045,4.8,10,0)
 for j in range(7):
  z=1.1+j*.48;r=1.15*(1-j*.105)
  cone(random.uniform(-.10,.10),random.uniform(-.10,.10),z,r,.025,1.3,11,1+j%3)
  for k in range(4):
   a=k*math.pi/2+j*.8;cone(math.cos(a)*r*.58,math.sin(a)*r*.58,z-.10,r*.46,.01,.85,7,1+(k+j)%3)
 me=bpy.data.meshes.new('Pine mesh %02d'%idx);me.from_pydata(verts,[],faces);me.materials.append(bark)
 for ma in [needles[idx%4],needles[(idx+1)%4],needles[(idx+2)%4]]:me.materials.append(ma)
 for poly,mi in zip(me.polygons,ids):poly.material_index=mi
 me.update();return me
trees=[tree_mesh(i) for i in range(5)]
for i in range(76):
 x=random.uniform(86,112);y=random.uniform(-16,18)
 # Slightly open clearing directly opposite the window.
 if x<90 and abs(y-1.15)<1.2:y+=2.0
 o=bpy.data.objects.new('Forest pine %03d'%i,trees[i%5]);current.objects.link(o);o.location=(x,y,-.4);q=random.uniform(.75,1.60);o.scale=(q,q,q);o.rotation_euler.z=random.uniform(0,6.28)
box('Forest floor',(100,1,-.53),(40,48,.20),earth,.02)
box('Forest distant haze',(116,1,9),(.2,60,20),sky,.02)
for i in range(24):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(random.uniform(85,110),random.uniform(-12,15),-.30));o=put(bpy.context.object,'Forest rock %02d'%i,stone);o.scale=(random.uniform(.2,.7),random.uniform(.2,.7),random.uniform(.15,.45))
# Window plane is the only entrance to the remote forest.
current=bpy.data.collections.new('12 Window portal');s.collection.children.link(current)
old=bpy.data.objects.get('Window glass');old.hide_render=True;old.hide_set(True)
portal=bpy.data.materials.new('WINDOW | Cycles Ray Portal');portal.use_nodes=True;nt=portal.node_tree;nt.nodes.clear()
out=nt.nodes.new('ShaderNodeOutputMaterial');p=nt.nodes.new('ShaderNodeBsdfRayPortal');geo=nt.nodes.new('ShaderNodeNewGeometry');add=nt.nodes.new('ShaderNodeVectorMath');add.operation='ADD';add.inputs[1].default_value=(offset,0,0);nt.links.new(geo.outputs['Position'],add.inputs[0]);nt.links.new(add.outputs[0],p.inputs['Position'])
# Back-facing window shows timber, never a portal seen from outside the room.
mix=nt.nodes.new('ShaderNodeMixShader');back=nt.nodes.new('ShaderNodeBsdfPrincipled');back.inputs['Base Color'].default_value=(.045,.020,.008,1);nt.links.new(geo.outputs['Backfacing'],mix.inputs[0]);nt.links.new(p.outputs[0],mix.inputs[1]);nt.links.new(back.outputs[0],mix.inputs[2]);nt.links.new(mix.outputs[0],out.inputs['Surface'])
me=bpy.data.meshes.new('Window portal plane');me.from_pydata([(2.57,.46,1.055),(2.57,.46,2.545),(2.57,1.84,2.545),(2.57,1.84,1.055)],[],[(0,1,2,3)]);me.materials.append(portal);me.update();o=bpy.data.objects.new('WINDOW_PORTAL - replace shader in Unity',me);current.objects.link(o)
o['unity_note']='Use forest-only camera with off-axis window projection and RenderTexture. Main camera excludes Forest layer. Portal origin offset +80m X in Blender.'
o['opening_width']=1.38;o['opening_height']=1.49;o['forest_offset_x']=80
current=bpy.data.collections['08 Lighting and Cameras']
def camera(name,loc,target):
 d=bpy.data.cameras.new(name);o=bpy.data.objects.new(name,d);current.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();d.lens=28;d.clip_start=.05;d.clip_end=250;return o
cam=camera('CAM 04 - Vintage window view',(-1.25,-1.75,1.72),(2.25,.95,1.48))
camera('CAM 05 - Window parallax left',(.65,.55,1.65),(2.57,1.15,1.7))
camera('CAM 06 - Window parallax right',(.65,1.35,1.65),(2.57,1.15,1.7))
data=bpy.data.lights.new('Forest afternoon sun','SUN');data.energy=2;data.angle=.12;data.color=(1,.83,.60);o=bpy.data.objects.new('Forest afternoon sun',data);current.objects.link(o);o.rotation_euler=(.5,-.6,-.4)
for name in ['Soft warm ceiling','Studio fill']:
 o=bpy.data.objects.get(name)
 if o:o.data.energy*=.8
s.world.node_tree.nodes.get('Background')
s.cycles.samples=24;s.cycles.use_denoising=True;s.cycles.transparent_max_bounces=32
s.camera=cam;s.render.resolution_x=1200;s.render.resolution_y=900;s.render.resolution_percentage=100
# Room-only layer for clean solid modeling; preview layer retains forest for Cycles.
preview=bpy.context.view_layer;preview.name='01 Portal Preview - Cycles'
model=s.view_layers.new('02 Modeling - room only')
def find(layer,name):
 if layer.name==name:return layer
 for child in layer.children:
  hit=find(child,name)
  if hit:return hit
find(model.layer_collection,'11 Forest - remote portal world').exclude=True
model.use=False
for a in bpy.context.screen.areas:
 if a.type=='VIEW_3D':a.spaces.active.shading.type='SOLID';a.spaces.active.shading.color_type='MATERIAL';a.spaces.active.region_3d.view_perspective='CAMERA';a.spaces.active.clip_end=250
s['vintage_revision']='All room hard surfaces wood; bedding flax/tobacco, brass and paper retained. Existing user transforms preserved.'
s['forest_instructions']='Cycles: use Portal Preview layer. Solid modeling: choose Modeling room-only layer (no forest geometry). Remote forest located +80 X. Unity requires its own portal rendering; shader does not transfer.'
s.render.filepath='E:/ProgramFiles/_Project/Puzzle/vintage_window.png'
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
print('SAVED',len(s.objects),'objects; forest trees=76; portal+room-only modeling layer ready')
