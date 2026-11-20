import bpy, math, random
from mathutils import Vector
random.seed(18)
scene=bpy.data.scenes.new('Poetry Room | Bedroom Escape')
bpy.context.window.scene=scene
scene.unit_settings.system='METRIC'
cols={}
for name in ['01 Architecture','02 Bed','03 Desk - Start and Final','04 Shelf - Sequence','05 Nightstand - State Lock','06 Wardrobe - Mirror','07 Props','08 Lighting and Cameras','09 Cutaway Shell']:
    c=bpy.data.collections.new(name); scene.collection.children.link(c); cols[name]=c
current=cols['01 Architecture']
def put(o,name,mat=None):
    o.name=name
    for c in list(o.users_collection): c.objects.unlink(o)
    current.objects.link(o)
    if mat: o.data.materials.append(mat)
    return o
def mat(name,col,metal=0,rough=.5):
    m=bpy.data.materials.new(name); m.diffuse_color=(*col,1); m.use_nodes=True
    p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'); p.inputs['Base Color'].default_value=(*col,1); p.inputs['Metallic'].default_value=metal; p.inputs['Roughness'].default_value=rough
    return m
oak=mat('Honey oak',(.32,.16,.065)); walnut=mat('Dark walnut',(.095,.045,.025)); green=mat('Sage plaster',(.25,.36,.32)); cream=mat('Ivory paint',(.73,.69,.56)); brass=mat('Aged brass',(.55,.34,.105),.72,.28); ink=mat('Ink',(.025,.055,.06)); paper=mat('Warm paper',(.84,.76,.56)); linen=mat('Linen',(.65,.59,.43)); teal=mat('Petrol quilt',(.055,.22,.23)); red=mat('Terracotta',(.39,.12,.065)); glass=mat('Mirror silver',(.73,.83,.83),1,.06)
wood=oak.node_tree; p=next(n for n in wood.nodes if n.type=='BSDF_PRINCIPLED'); tex=wood.nodes.new('ShaderNodeTexNoise'); tex.inputs['Scale'].default_value=4; tex.inputs['Detail'].default_value=2
coord=wood.nodes.new('ShaderNodeTexCoord'); mapping=wood.nodes.new('ShaderNodeVectorMath'); mapping.operation='MULTIPLY'; mapping.inputs[1].default_value=(3,35,3); wood.links.new(coord.outputs['Generated'],mapping.inputs[0]); wood.links.new(mapping.outputs[0],tex.inputs['Vector'])
ramp=wood.nodes.new('ShaderNodeValToRGB'); ramp.color_ramp.elements[0].color=(.13,.055,.018,1); ramp.color_ramp.elements[1].color=(.43,.24,.105,1); wood.links.new(tex.outputs['Fac'],ramp.inputs[0]); wood.links.new(ramp.outputs[0],p.inputs['Base Color'])
def box(name,loc,size,ma,bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=put(bpy.context.object,name,ma); o.dimensions=size; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Soft edges','BEVEL'); mod.width=bevel; mod.segments=3
        o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    return o
def cyl(name,loc,r,depth,ma,rot=(0,0,0),vertices=48):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=depth,location=loc,rotation=rot); o=put(bpy.context.object,name,ma); b=o.modifiers.new('Edge radius','BEVEL'); b.width=.008; b.segments=3; o.modifiers.new('Weighted normals','WEIGHTED_NORMAL'); return o
def sphere(name,loc,size,ma):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=loc); o=put(bpy.context.object,name,ma); o.scale=size
    for p in o.data.polygons:p.use_smooth=True
    return o
def text(name,body,loc,size,ma,rot=(0,0,0)):
    bpy.ops.object.text_add(location=loc,rotation=rot); o=put(bpy.context.object,name,ma); o.data.body=body; o.data.size=size; o.data.extrude=.0004; return o
def rod(name,a,b,r,ma):
    a,b=Vector(a),Vector(b); o=cyl(name,(a+b)/2,r,(b-a).length,ma); o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler(); return o
def tag(o,stage,role):o['puzzle_stage']=stage;o['interaction_role']=role;o['implementation']='Visual prototype; connect in game engine'
# Floorboards with staggered joints.
box('Foundation',(0,0,-.13),(5.2,5.2,.24),walnut)
for row in range(20):
    y=-2.375+row*.25
    cuts=[-2.5]+([-1.9,-.65,.6,1.85] if row%2 else [-1.25,0,1.25])+[2.5]
    for j in range(len(cuts)-1): box('Oak floorboard',((cuts[j]+cuts[j+1])/2,y,.008),(cuts[j+1]-cuts[j]-.012,.238,.035),oak,.004)
box('North wall',(0,2.58,1.5),(5.2,.16,3),green)
# East wall has a real window opening, y=.45..1.85, z=1.05..2.55.
box('East wall south',(2.58,-1.025,1.5),(.16,2.95,3),cream)
box('East wall north',(2.58,2.175,1.5),(.16,.65,3),cream)
box('Window lower wall',(2.58,1.15,.525),(.16,1.4,1.05),cream)
box('Window upper wall',(2.58,1.15,2.775),(.16,1.4,.45),cream)
for z,h in [(.15,.26),(.8,.035)]:
    box('North panel rail',(0,2.475,z),(5,.045,h),walnut,.006)
    box('East panel rail',(2.475,0,z),(.045,5,h),walnut,.006)
for x in range(-2,3):box('North panel stile',(x,2.47,.46),(.028,.04,.6),walnut,.003)
for y in [-2,-1,0,1,2]:box('East panel stile',(2.47,y,.46),(.04,.028,.6),walnut,.003)
box('North crown',(0,2.45,2.91),(5,.1,.12),cream)
box('East crown',(2.45,0,2.91),(.1,5,.12),cream)
current=cols['09 Cutaway Shell']
box('West wall - enable for enclosed view',(-2.58,0,1.5),(.16,5.2,3),green)
box('South wall left',(-.925,-2.58,1.5),(3.15,.16,3),cream)
box('South wall right',(2.225,-2.58,1.5),(.55,.16,3),cream)
box('Door lintel',(1.3,-2.58,2.64),(1.3,.16,.72),cream)
box('Ceiling',(0,0,3.08),(5.2,5.2,.16),cream)
current=cols['01 Architecture']
for y in [.44,1.86]:box('Window jamb',(2.48,y,1.8),(.16,.085,1.6),walnut)
for z in [1.04,1.8,2.56]:box('Window crossbar',(2.48,1.15,z),(.16,1.5,.065),walnut)
box('Window mullion',(2.48,1.15,1.8),(.15,.045,1.5),walnut)
sky=mat('Blue dusk glass',(.16,.3,.42),.15,.28)
box('Window glass',(2.575,1.15,1.8),(.015,1.36,1.46),sky,.002)
box('Window sill',(2.35,1.15,1.02),(.42,1.65,.08),oak)
rod('Curtain rod',(2.28,.15,2.69),(2.28,2.15,2.69),.022,brass)
for side in [.25,1.87]:
    for i in range(5):
        o=cyl('Curtain fold',(2.30+(i%2)*.025,side+i*.045,1.84),.06,1.65,linen); o.scale.x=.65
door=box('EXIT - hinged door',(1.3,-2.505,1.12),(1.14,.075,2.24),oak);tag(door,5,'Exit using brass key')
for x in [.68,1.92]:box('Door casing',(x,-2.43,1.15),(.09,.12,2.3),cream)
box('Door top casing',(1.3,-2.43,2.3),(1.33,.12,.09),cream)
for z in [.56,1.57]:box('Door recessed panel',(1.3,-2.453,z),(.88,.018,.79),walnut,.012);box('Door panel inset',(1.3,-2.437,z),(.80,.018,.71),oak,.012)
cyl('Door escutcheon',(1.73,-2.39,1.03),.055,.025,brass,(math.pi/2,0,0));sphere('Door knob',(1.73,-2.35,1.08),(.045,.04,.045),brass)
box('Keyhole',(1.73,-2.37,1.005),(.012,.008,.024),ink,.003)
# Bed.
current=cols['02 Bed']
box('Bed frame',(-.65,1.19,.34),(1.56,2.25,.24),walnut)
for x in [-1.29,-.01]:
    for y in [.23,2.13]:box('Bed foot',(x,y,.19),(.12,.12,.35),oak)
box('Headboard',(-.65,2.31,.86),(1.65,.12,1.32),oak,.045)
box('Headboard upholstered panel',(-.65,2.23,.98),(1.42,.08,.75),linen,.065)
box('Mattress',(-.65,1.19,.56),(1.47,2.12,.25),cream,.105)
box('Duvet',(-.65,.86,.73),(1.49,1.48,.19),teal,.075)
for x in [-1.32,-1.26,-.04,.02]:box('Duvet stitched border',(x,.86,.832),(.012,1.30,.004),linen,.002)
for x in [-1.02,-.3]:box('Pillow',(x,1.95,.76),(.63,.43,.17),linen,.075)
box('Folded throw',(-.65,.32,.85),(1.5,.38,.075),red,.035)
for i in range(18):rod('Throw fringe',(-1.34+i*.08,.10,.86),(-1.34+i*.08,.04,.82),.004,linen)
current=cols['07 Props']
box('Woven rug',(.2,-.55,.044),(2.1,2.25,.018),linen,.025)
for x in [-.76,1.16]:box('Rug border',(x,-.55,.056),(.035,2.12,.005),teal,.001)
for y in [-1.61,.51]:box('Rug border',(.2,y,.056),(1.92,.035,.005),teal,.001)
# Desk along east wall, top height .8m.
current=cols['03 Desk - Start and Final']
box('Desk top',(2,-.92,.81),(.87,1.65,.09),oak)
for x in [1.65,2.34]:
    for y in [-1.62,-.22]:box('Desk leg',(x,y,.40),(.075,.075,.78),walnut)
box('Drawer pedestal',(2,-1.38,.48),(.72,.47,.57),walnut)
for z in [.33,.57]:
    d=box('Final drawer front' if z==.33 else 'Desk upper drawer',(1.625,-1.38,z),(.045,.43,.20),oak)
    tag(d,5,'Word choice drawer - poem text pending')
    rod('Drawer pull',(1.585,-1.46,z),(1.585,-1.30,z),.014,brass)
for i in range(3):box('Word lock tile - replace text',(1.59,-1.52+i*.14,.42),(.025,.105,.055),brass,.006)
letter=box('START - unfinished letter',(1.88,-.90,.867),(.40,.30,.008),paper,.002);tag(letter,1,'Poem quotation placeholder - replace after source supplied')
for i in range(5):box('Letter handwriting placeholder',(1.87,-.8-i*.035,.873),(.27-(i%3)*.03,.005,.001),ink,.0005)
cyl('Wax seal',(1.96,-1.01,.877),.026,.006,red)
rod('Pen',(1.66,-.87,.875),(1.68,-1.13,.875),.008,ink)
cyl('Desk lamp base',(2.1,-.35,.87),.13,.035,brass)
rod('Lamp stem',(2.1,-.35,.89),(2.1,-.35,1.33),.015,brass)
bpy.ops.mesh.primitive_cone_add(vertices=48,radius1=.19,radius2=.09,depth=.24,location=(2.1,-.35,1.39));put(bpy.context.object,'Linen lamp shade',cream)
box('Chair seat',(1.18,-.95,.46),(.48,.48,.08),oak)
for x in [.99,1.37]:
    for y in [-1.14,-.76]:box('Chair leg',(x,y,.24),(.055,.055,.45),walnut)
for y in [-1.14,-.76]:box('Chair back upright',(.98,y,.78),(.045,.045,.62),walnut)
box('Chair back',(.98,-.95,.95),(.055,.43,.19),oak)
# Bookshelf on west wall, facing +X.
current=cols['04 Shelf - Sequence']
for y in [-1.82,-.57]:box('Bookcase side',(-2.19,y,.57),(.49,.06,1.1),oak)
for z in [.1,.42,.78,1.1]:box('Bookcase shelf',(-2.19,-1.195,z),(.49,1.31,.06),oak)
box('Bookcase back',(-2.43,-1.195,.59),(.035,1.3,1.05),walnut)
for i in range(13):
    h=random.uniform(.19,.29);ma=[teal,red,linen,ink][i%4]
    box('Book spine',(-2.18,-1.72+i*.083,.46+h/2),(.32,.065,h),ma,.004)
    for z in [.49,.49+h-.06]:box('Book gilt band',(-2.013,-1.72+i*.083,z),(.004,.058,.007),brass,.001)
d=box('Sequence reward drawer',(-1.945,-1.195,.22),(.035,1.17,.17),oak);tag(d,2,'Opens on correct token order; contains knob and photograph')
rod('Shelf drawer handle',(-1.91,-1.31,.23),(-1.91,-1.08,.23),.012,brass)
for i in range(4):
    y=-1.68+i*.32
    o=cyl('Movable poem token %02d'%(i+1),(-2.18,y,1.15),.105,.035,brass);tag(o,2,'Reorder using poem; symbolic placeholders')
    text('Token number',str(i+1),(-2.10,y-.02,1.174),.05,ink,(0,0,math.pi/2))
    if i==0:sphere('Token - stone',(-2.2,y,1.23),(.07,.06,.065),cream)
    if i==1:rod('Token - stem',(-2.2,y,1.17),(-2.2,y,1.39),.009,brass);sphere('Token - leaf',(-2.2,y+.035,1.31),(.015,.065,.026),teal)
    if i==2:
        box('Token - house',(-2.2,y,1.23),(.10,.10,.11),red)
        bpy.ops.mesh.primitive_cone_add(vertices=4,radius1=.10,radius2=0,depth=.08,location=(-2.2,y,1.32),rotation=(0,0,math.pi/4));put(bpy.context.object,'Token - roof',walnut)
    if i==3:sphere('Token - moon',(-2.2,y,1.27),(.075,.025,.075),brass)
# Nightstand.
current=cols['05 Nightstand - State Lock']
box('Nightstand body',(-1.92,1.84,.43),(.65,.65,.7),walnut)
box('Nightstand top',(-1.92,1.84,.81),(.72,.72,.07),oak)
o=box('Locked bedside drawer',(-1.92,1.499,.52),(.57,.045,.25),oak);tag(o,3,'State puzzle; opens to mirror overlay and wardrobe key')
rod('Bedside pull',(-2.02,1.46,.48),(-1.82,1.46,.48),.013,brass)
for i in range(3):
    x=-2.13+i*.21
    o=cyl('State dial %02d'%(i+1),(x,1.73,.866),.076,.025,brass);tag(o,3,'Three-state dial; exact symbols pending poem')
    rod('Dial pointer',(x,1.73,.883),(x+.044,1.73,.883),.006,ink)
    for k in range(3):
        ang=k*2*math.pi/3;cyl('Dial state marker',(x+math.cos(ang)*.058,1.73+math.sin(ang)*.058,.885),.007,.003,ink,vertices=12)
box('Old photograph',(-1.94,1.99,.86),(.25,.16,.008),paper,.002)
box('Photograph image',(-1.94,1.99,.866),(.21,.12,.002),teal,.002)
# Wardrobe open panel and inset mirror.
current=cols['06 Wardrobe - Mirror']
for y in [.15,1.32]:box('Wardrobe side',(-2.16,y,1.11),(.62,.07,2.18),walnut)
for z in [.12,2.20]:box('Wardrobe cap',(-2.16,.735,z),(.69,1.29,.09),oak)
box('Wardrobe back',(-2.46,.735,1.15),(.04,1.18,2.12),oak)
box('Wardrobe closed panel',(-1.82,1.02,1.15),(.055,.53,1.98),oak)
sphere('Wardrobe handle',(-1.76,.84,1.15),(.025,.025,.025),brass)
rod('Hanging rail',(-2.15,.22,1.88),(-2.15,.70,1.88),.017,brass)
for i in range(3):box('Folded linen',(-2.15,.43,.24+i*.10),(.42,.37,.085),[linen,teal,cream][i],.025)
# Door pivot retained for engine animation.
bpy.ops.object.empty_add(type='PLAIN_AXES',location=(-1.80,.16,1.15));pivot=put(bpy.context.object,'Wardrobe mirror door hinge')
o=box('Wardrobe open door',(-1.52,.16,1.15),(.58,.06,1.98),oak);o.parent=pivot;o.matrix_parent_inverse=pivot.matrix_world.inverted()
o=box('Inset mirror',(-1.52,.119,1.22),(.45,.016,1.55),glass,.018);o.parent=pivot;o.matrix_parent_inverse=pivot.matrix_world.inverted();tag(o,4,'Insert transparent fragment; reflected instruction to read poem ending')
for x in [-1.77,-1.27]:box('Mirror brass edge',(x,.10,1.22),(.018,.022,1.60),brass,.004)
for z in [.42,2.02]:box('Mirror brass edge',(-1.52,.10,z),(.51,.022,.018),brass,.004)
box('Overlay insertion slot',(-1.52,.087,.57),(.25,.025,.035),brass,.004)
# Wall artwork and small lived-in props.
current=cols['07 Props']
box('Poetry print frame',(.93,2.455,1.97),(.67,.055,.83),walnut)
box('Poetry print paper',(.93,2.42,1.97),(.58,.012,.74),paper,.003)
text('Print title','AFTER\nTHE LAST\nLINE',(.69,2.405,2.16),.092,ink,(math.pi/2,0,0))
for i in range(5):box('Print decorative line',(.93,2.402,1.88-i*.045),(.39-i*.035,.005,.007),teal,.001)
cyl('Window plant pot',(2.32,1.57,1.16),.10,.22,red)
for i in range(7):
    ang=i*2.4;a=(2.32,1.57,1.24);b=(2.32+.14*math.cos(ang),1.57+.14*math.sin(ang),1.40+(i%3)*.06);rod('Plant stem',a,b,.006,teal);sphere('Plant leaf',b,(.075,.035,.025),teal)
# Key stored inside the final drawer.
current=cols['03 Desk - Start and Final']
bpy.ops.mesh.primitive_torus_add(major_radius=.026,minor_radius=.006,location=(1.95,-1.38,.32));o=put(bpy.context.object,'Final brass key bow',brass);tag(o,5,'Exit key reward')
rod('Key shaft',(1.97,-1.38,.32),(2.07,-1.38,.32),.006,brass)
box('Key teeth',(2.05,-1.365,.32),(.03,.026,.012),brass,.002)
# Cameras and lighting.
current=cols['08 Lighting and Cameras']
def aim(o,target):o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
def area(name,loc,target,power,color,size):
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.color=color;data.shape='DISK';data.size=size;o=bpy.data.objects.new(name,data);current.objects.link(o);o.location=loc;aim(o,target);return o
area('Soft warm ceiling',(0,-.2,2.85),(0,0,0),360,(1,.80,.59),3.5)
area('Cool window light',(2.35,1.15,2.2),(-1,0,.7),180,(.58,.74,1),1.25)
area('Studio fill',(-3,-4,5),(0,0,.5),480,(1,.90,.75),5)
data=bpy.data.lights.new('Lamp practical','POINT');data.energy=18;data.color=(1,.61,.25);data.shadow_soft_size=.10;o=bpy.data.objects.new('Lamp practical',data);current.objects.link(o);o.location=(2.1,-.35,1.29)
def camera(name,loc,target,lens=28,ortho=None):
    d=bpy.data.cameras.new(name);o=bpy.data.objects.new(name,d);current.objects.link(o);o.location=loc;aim(o,target);d.lens=lens;d.clip_start=.05
    if ortho:d.type='ORTHO';d.ortho_scale=ortho
    return o
cam=camera('CAM 01 - Cutaway',(-7.5,-9,7.7),(0,0,1),ortho=8.5)
camera('CAM 02 - Player entry',(1.25,-2.12,1.62),(-.5,1.0,1.1),23)
camera('CAM 03 - Desk clue',(.5,-1.65,1.65),(1.93,-.9,.82),40)
scene.camera=cam
world=bpy.data.worlds.new('Evening ambient');world.use_nodes=True;next(n for n in world.node_tree.nodes if n.type=='BACKGROUND').inputs[0].default_value=(.16,.20,.23,1);next(n for n in world.node_tree.nodes if n.type=='BACKGROUND').inputs[1].default_value=.35;scene.world=world
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1400;scene.render.resolution_y=1400;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
shell=cols['09 Cutaway Shell'];shell.hide_render=True;shell.hide_viewport=True
scene['README']='5 x 5 x 3m bedroom. Shell hidden for cutaway; enable collection 09 for enclosed view. Props are poem placeholders; no gameplay logic yet. Original startup scene retained.'
scene['puzzle_chain']='Desk letter > shelf token order > bedside state dials > wardrobe mirror > desk final drawer > exit'
for screen in bpy.data.screens:
    for a in screen.areas:
        if a.type=='VIEW_3D':
            a.spaces.active.region_3d.view_perspective='CAMERA'
scene.render.filepath='E:/ProgramFiles/_Project/Puzzle/bedroom_overview.png'
bpy.ops.wm.save_as_mainfile(filepath='E:/ProgramFiles/_Project/Puzzle/Poetry_Escape_Room.blend')
print('Created',len(scene.objects),'objects; saved Poetry_Escape_Room.blend')

