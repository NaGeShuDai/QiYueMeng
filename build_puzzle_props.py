import bpy,math,random,json
from mathutils import Vector
random.seed(42)
BASE='E:/ProgramFiles/_Project/Puzzle/PuzzleProps/'
scene=bpy.data.scenes.new('Poetry Escape | Puzzle prop library')
bpy.context.window.scene=scene;scene.unit_settings.system='METRIC'
font=bpy.data.fonts.load('C:/Windows/Fonts/msyh.ttc')
def material(name,color,metal=0,rough=.5):
 m=bpy.data.materials.new('PP_'+name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
 return m
wood=material('AntiqueWalnut',(.26,.12,.045));dark=material('DarkWood',(.055,.022,.009));brass=material('AgedBrass',(.36,.22,.065),.72,.32)
ivory=material('Bone',(.72,.61,.40));ink=material('BlackenedMetal',(.018,.025,.024),.4,.42);red=material('RedOchre',(.38,.075,.035));green=material('Olive',(.10,.19,.12));water=material('RiverEnamel',(.04,.19,.23),.25,.28);paper=material('Paper',(.79,.68,.45));yellow=material('AmberGlass',(.65,.34,.055),.2,.24)
img=bpy.data.images.load(BASE+'Textures/AntiqueWalnut_BaseColor.png',check_existing=True)
nt=wood.node_tree;p=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED');t=nt.nodes.new('ShaderNodeTexImage');t.image=img;nt.links.new(t.outputs['Color'],p.inputs['Base Color'])
roots=[];inventory=[];current=None;parent=None
def collection(name,offset):
 global current,parent
 current=bpy.data.collections.new(name);scene.collection.children.link(current)
 parent=bpy.data.objects.new(name,None);current.objects.link(parent);parent.location=offset;parent.empty_display_size=.08;roots.append(parent)
 parent['asset_id']=name;parent['units']='meters';parent['gameplay']='Model only. Unity owns interaction and state.'
 return parent
def put(o,name,ma=None,par=None):
 o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 current.objects.link(o)
 if ma:o.data.materials.append(ma)
 o.parent=par if par else parent
 return o
def box(name,loc,size,ma=wood,bevel=.008,par=None):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=put(bpy.context.object,name,ma,par);o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  m=o.modifiers.new('Crafted edge','BEVEL');m.width=bevel;m.segments=2;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o
def cyl(name,loc,r,depth,ma=brass,rot=(0,0,0),par=None,n=32):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=depth,location=loc,rotation=rot);o=put(bpy.context.object,name,ma,par);m=o.modifiers.new('Rim bevel','BEVEL');m.width=.002;m.segments=2;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');return o
def sphere(name,loc,scale,ma=ivory,par=None):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,location=loc);o=put(bpy.context.object,name,ma,par);o.scale=scale
 for p in o.data.polygons:p.use_smooth=True
 return o
def cone(name,loc,r1,r2,h,ma,par=None,n=24):
 bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=h,location=loc);o=put(bpy.context.object,name,ma,par);o.modifiers.new('Normals','WEIGHTED_NORMAL');return o
def rod(name,a,b,r=.006,ma=brass,par=None):
 a,b=Vector(a),Vector(b);o=cyl(name,(a+b)/2,r,(b-a).length,ma,par=par);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def text(name,body,loc,size=.025,ma=ivory,rot=(0,0,0),par=None):
 bpy.ops.object.text_add(location=loc,rotation=rot);o=put(bpy.context.object,name,ma,par);o.data.body=body;o.data.font=font;o.data.size=size;o.data.align_x='CENTER';o.data.align_y='CENTER';o.data.extrude=.00035;o.data.resolution_u=4;return o
def empty(name,loc=(0,0,0),par=None,kind=None):
 o=bpy.data.objects.new(name,None);current.objects.link(o);o.parent=par if par else parent;o.location=loc;o.empty_display_size=.035
 if kind:o['interaction']=kind
 return o
def movable(name,loc,kind,axis,value,par=None):
 o=empty(name,loc,par,kind);o['local_axis']=axis;o['open_amount']=value;o['closed_pose']='authored local transform';return o
def reparent_local(o,pivot):
 # Both share the same asset-root space before this call.
 old=o.location.copy();o.parent=pivot;o.location=old-pivot.location
def tray(name,center,size,ma=wood,par=None):
 x,y,z=center;w,d,h=size
 box(name+'_Bottom',(x,y,z+.008),(w,d,.016),ma,par=par)
 box(name+'_Front',(x,y-d/2+.01,z+h/2),(w,.02,h),ma,par=par)
 box(name+'_Back',(x,y+d/2-.01,z+h/2),(w,.02,h),ma,par=par)
 for side in [-1,1]:box(name+'_Side',(x+side*(w/2-.01),y,z+h/2),(.02,d-.04,h),ma,par=par)
def plaque(label,loc,size=(.20,.07),par=None):
 box('Plaque_'+label,loc,(size[0],size[1],.005),brass,.003,par)
 text('Engraving_'+label,label,(loc[0],loc[1],loc[2]+.003),min(.027,size[1]*.36),ink,par=par)
def screw(loc,par=None):
 cyl('Brass screw',loc,.007,.004,brass,par=par,n=12);box('Screw slot',(loc[0],loc[1],loc[2]+.0025),(.009,.0015,.001),ink,.0003,par)
def key(name,loc):
 k=empty(name,loc,kind='pickup');k['item_id']='winding_key'
 bpy.ops.mesh.primitive_torus_add(major_radius=.022,minor_radius=.005,major_segments=24,minor_segments=8,location=(0,0,.008));put(bpy.context.object,'Key bow',brass,k)
 rod('Key shaft',(0,-.018,.008),(0,-.105,.008),.006,brass,k)
 box('Key tooth',(.009,-.089,.008),(.023,.012,.012),brass,.002,k);return k
def block(char,loc,stage):
 p=empty('Pickup_WoodBlock_'+char,loc,kind='pickup');p['item_id']='wood_block_'+char;p['reward_stage']=stage
 box('Letter block '+char,(0,0,.019),(.085,.085,.038),wood,.006,p)
 box('Letter inset '+char,(0,0,.039),(.068,.068,.003),dark,.002,p)
 text('Carved character '+char,char,(0,0,.042),.052,ivory,par=p)
 inventory.append(p);return p
def lid(name,loc,w,d,angle=0):
 p=movable(name,loc,'hinge','X',105)
 box(name+'_Panel',(0,-d/2,.012),(w,d,.024),wood,.007,p)
 for x in [-w*.33,w*.33]:cyl('Lid hinge',(x,0,.01),.012,.07,brass,(0,math.pi/2,0),p)
 p.rotation_euler.x=math.radians(angle);return p
def reward_drawer(name,center,w,d,h=.07):
 p=movable(name,center,'slide','-Y',.18);tray(name,(0,0,0),(w,d,h),par=p)
 rod('Drawer pull',(-.04,-d/2-.014,h*.5),(.04,-d/2-.014,h*.5),.006,brass,p);return p
def dial(name,loc,values,r,answer):
 p=movable(name,loc,'indexed_rotation','Z',360/len(values));p['values']='|'.join(values);p['solution']=answer;p['step_degrees']=360/len(values)
 cyl(name+'_Rim',(0,0,0),r,.025,brass,par=p);cyl(name+'_Face',(0,0,.015),r*.91,.006,ivory,par=p)
 for i,v in enumerate(values):
  a=math.pi/2+i*2*math.pi/len(values);tx=math.cos(a)*r*.69;ty=math.sin(a)*r*.69
  text(name+'_'+v,v,(tx,ty,.020),r*.30,ink,(0,0,a-math.pi/2),p)
 cyl('Dial hub',(0,0,.022),r*.20,.009,wood,par=p)
 box('Fixed reading marker',(loc[0],loc[1]+r+.008,loc[2]+.018),(.012,.013,.007),red,.002)
 return p
# P01: lock module for existing desk drawer.
r=collection('P01_NumberLock',( -1.65,1.55,0))
box('Lock mounting block',(0,0,.04),(.47,.24,.08),wood)
box('Lock brass face',(0,0,.086),(.44,.21,.012),brass)
for i in range(3):dial('DigitWheel_%d'%i,(-.145+i*.145,0,.11),[str(j) for j in range(10)],.060,str(i+1))
for x in [-.20,.20]:
 for y in [-.085,.085]:screw((x,y,.096))
confirm=movable('Lock release catch',(0,-.145,.045),'press','Y',.012);box('Catch button',(0,0,0),(.13,.035,.035),dark,par=confirm)
empty('MountAnchor_DrawerFront',(0,0,0));empty('RewardAnchor_Key',(.30,0,.02));key('Pickup_WindingKey',(.31,-.02,.015));block('第',(-.32,-.02,0),1)
plaque('01 · 锁',(0,-.24,.008))
# P02: river crossing mechanical tabletop.
r=collection('P02_RiverCrossing',(-.52,1.55,0))
tray('River box',(0,0,0),(.78,.58,.10));box('River bed',(0,0,.10),(.74,.54,.025),water)
for x in [-.28,.28]:box('River bank',(x,0,.13),(.18,.52,.035),green)
for j in range(5):
 y=-.20+j*.095;rod('Enamel river ripple',(-.15,y,.116),(.15,y+.018,.116),.0025,ivory)
rod('Boat guide rail',(-.25,-.08,.144),(.25,-.08,.144),.004,brass)
boat=movable('Boat_Carriage',(-.21,-.08,.16),'slide','X',.42);boat['cargo_capacity']=1
box('Boat bottom',(0,0,.012),(.115,.17,.025),wood,par=boat)
for x in [-.052,.052]:box('Boat side',(x,0,.035),(.016,.18,.055),wood,par=boat)
for y in [-.085,.085]:box('Boat end',(0,y,.035),(.11,.016,.055),wood,par=boat)
empty('CargoSocket',(0,0,.05),boat,'install');sphere('Boatman head',(0,-.05,.10),(.022,.022,.025),ivory,boat);cone('Boatman body',(0,-.05,.067),.022,.013,.045,dark,boat)
crank=movable('River_Crank',(-.32,-.29,.12),'rotation','Y',360);cyl('Crank hub',(0,0,0),.025,.025,brass,(math.pi/2,0,0),crank);rod('Crank arm',(0,-.02,0),(.06,-.02,0),.006,brass,crank);cyl('Crank grip',(.06,-.04,0),.009,.05,wood,(math.pi/2,0,0),crank)
empty('WindingKeySocket',(-.32,-.29,.12),kind='install');box('Winding keyhole',(-.32,-.308,.12),(.015,.003,.008),ink,.001)
for idx,name in enumerate(['Fox','Chicken','Grain']):
 p=empty('Token_'+name,(-.28,.025+idx*.09,.16),kind='movable_token');p['bank']='left';cyl(name+' base',(0,0,.005),.033,.01,brass,par=p)
 if name=='Fox':
  sphere('Fox body',(0,0,.04),(.025,.018,.03),red,p);sphere('Fox head',(0,-.006,.075),(.023,.021,.022),red,p)
  for x in [-.014,.014]:cone('Fox ear',(x,0,.10),.012,0,.028,red,p,n=3);sphere('Fox eye',(x,-.025,.08),(.003,.003,.003),ink,p)
  sphere('Fox muzzle',(0,-.026,.069),(.012,.015,.009),ivory,p);o=sphere('Fox tail',(.025,.015,.04),(.014,.013,.035),red,p);o.rotation_euler.y=.6
 elif name=='Chicken':
  sphere('Chicken body',(0,0,.04),(.027,.024,.028),ivory,p);sphere('Chicken head',(0,-.015,.068),(.016,.016,.019),ivory,p);cone('Chicken comb',(0,-.013,.092),.010,0,.021,red,p,n=4);rod('Chicken beak',(0,-.026,.067),(0,-.040,.065),.005,yellow,p)
 else:
  sphere('Grain sack',(0,0,.038),(.025,.022,.033),paper,p);cyl('Sack tie',(0,0,.064),.015,.009,dark,par=p);text('Grain label','麦',(0,-.022,.038),.025,ink,(math.pi/2,0,0),p)
 empty('BankSocket_Right_'+name,(.28,.025+idx*.09,.16),kind='install')
tray('Reward boathouse',(.27,-.16,.14),(.14,.14,.15),dark)
roof=box('Boathouse roof',(.27,-.16,.30),(.18,.18,.025),wood);roof.rotation_euler.y=.15
p=movable('Boathouse_Door',(.20,-.235,.22),'hinge','Z',-110);box('Boathouse door',(.07,0,0),(.14,.018,.15),wood,par=p)
empty('Reward_Compass',(.27,-.16,.19));plaque('狐 → 鸡 → 麦',(0,.35,.03),(.37,.065))
text('River rule','船到岸边才安全',(0,.405,.033),.021,ivory)
plaque('02 · 渡河',(0,-.40,.008))
# P03 compasses as flat cabinet-mounted board, third on display beside empty socket.
r=collection('P03_CompassBoard',(.65,1.55,0))
box('Compass mount',(0,0,.045),(.64,.31,.09));box('Compass dark face',(0,0,.095),(.60,.28,.012),dark)
for i in range(3):
 x=-.205+i*.205;cyl('Socket recess',(x,0,.105),.086,.009,ink)
 for k in range(4):
  a=k*math.pi/2;screw((x+.081*math.cos(a),.081*math.sin(a),.112))
 empty('CompassSocket_%d'%i,(x,0,.115),kind='install');text('Compass association',['日','屋','月'][i],(x,.116,.112),.032,ivory)
compass_roots=[]
for i in range(3):
 pos=(-.205+i*.205,0,.125) if i<2 else (.39,-.05,.035)
 cp=empty('Compass_%d'%i,pos,kind='pickup' if i==2 else 'fixed_component');cp['item_id']='missing_compass' if i==2 else 'installed_compass';compass_roots.append(cp)
 cyl('Compass casing',(0,0,0),.076,.025,brass,par=cp);cyl('Compass face',(0,0,.016),.067,.007,ivory,par=cp)
 for a,label in [(math.pi/2,'N'),(0,'E'),(-math.pi/2,'S'),(math.pi,'W')]:text('Compass cardinal '+label,label,(math.cos(a)*.051,math.sin(a)*.051,.022),.021,ink,par=cp)
 for k in range(16):
  a=k*math.pi/8;rod('Compass tick',(math.cos(a)*.057,math.sin(a)*.057,.022),(math.cos(a)*.062,math.sin(a)*.062,.022),.0012,ink,cp)
 needle=movable('CompassNeedle_%d'%i,(0,0,.03),'indexed_rotation','Z',90,cp);needle['solution']=['E','N','W'][i]
 for sign,ma in [(1,red),(-1,ink)]:
  me=bpy.data.meshes.new('Needle triangle');me.from_pydata([(-.009,0,0),(.009,0,0),(0,.041*sign,0)],[],[(0,1,2)]);o=bpy.data.objects.new('Needle',me);current.objects.link(o);o.parent=needle;me.materials.append(ma)
 cyl('Needle pin',(0,0,.033),.006,.007,brass,par=cp)
reward_drawer('CompassRewardDrawer',(0,-.015,.01),.40,.23,.055)
block('七',(-.32,-.25,0),3);empty('Reward_LightBulb',(0,-.04,.035));plaque('03 · 定向',(0,-.30,.008))
# P04 wiring lamp with 3x3 ceramic rotary tiles.
r=collection('P04_WiringLamp',(1.85,1.55,0))
tray('Lamp puzzle base',(0,0,0),(.58,.55,.10));box('Circuit inset',(0,-.025,.105),(.53,.43,.015),dark)
for x in [-.25,.25]:
 for y in [-.20,.16]:screw((x,y,.117))
path={ (0,1):['W','N'],(0,0):['S','E'],(1,0):['W','S'],(1,1):['N','E'],(2,1):['W','S'],(2,2):['N','E'] }
vectors={'N':(0,1),'E':(1,0),'S':(0,-1),'W':(-1,0)}
scramble=[1,2,1,3,2,0,2,1,3]
for row in range(3):
 for col in range(3):
  k=row*3+col;x=-.16+col*.16;y=.12-row*.145
  tile=movable('WireTile_%d'%k,(x,y,.128),'indexed_rotation','Z',90);tile['solution_rotation']=0;tile['initial_quarter_turns']=scramble[k]
  box('Ceramic tile',(0,0,0),(.135,.125,.018),ivory,.009,tile)
  dirs=path.get((col,row),['N','S']);tile['ports']=''.join(dirs)
  for direction in dirs:
   dx,dy=vectors[direction];rod('Copper conductor',(0,0,.014),(dx*.065,dy*.060,.014),.006,brass,tile)
  cyl('Tile contact',(0,0,.015),.011,.005,brass,par=tile)
  if (col,row) in [(0,0),(1,1),(2,2)]:cyl('Checkpoint light',(0,0,.020),.006,.005,yellow,par=tile);tile['checkpoint']=True
  tile.rotation_euler.z=scramble[k]*math.pi/2
rod('Source lead',(-.32,-.025,.14),(-.23,-.025,.14),.008,brass);text('Source symbol','＋',(-.32,-.025,.16),.035,ivory)
rod('Output lead',(.23,-.17,.14),(.31,-.17,.14),.007,brass)
rod('Lamp stem',(.23,.20,.10),(.23,.20,.50),.013,brass)
cyl('Lamp socket',(.23,.20,.52),.030,.05,ink);empty('LightBulbSocket',(.23,.20,.55),kind='install')
shade=empty('LampShade_Assembly',(.23,.20,.67));cone('Brass shade',(0,0,0),.14,.045,.16,brass,shade)
bulb=empty('Pickup_LightBulb',(.39,-.07,.015),kind='pickup');bulb['item_id']='light_bulb'
cyl('Bulb screw base',(0,0,.02),.020,.04,brass,par=bulb)
for j in range(5):cyl('Bulb thread',(0,0,.005+j*.007),.022,.002,ink,par=bulb)
sphere('Bulb envelope',(0,0,.065),(.034,.034,.045),yellow,bulb)
reward_drawer('LampRewardDrawer',(0,.01,.012),.44,.42,.065);empty('Reward_WordBox',(0,0,.03))
plaque('04 · 航灯',(0,-.38,.008))
# P05 four-letter word box, lid shown partially raised for review.
r=collection('P05_WordBox',(-1.65,.15,0))
tray('Word box',(0,0,0),(.49,.29,.13));cover=lid('WordBox_Lid',(0,.145,.13),.49,.29,0)
for i,letters in enumerate(['LABCDE','OAEIUR','VNOPQR','EAIOTY']):
 x=-.18+i*.12;p=dial('LetterWheel_%d'%i,(x,-.01,.18),list(letters),.05,'LOVE'[i]);p.parent=cover;p.location=(x,-.155,.05)
 # Reading marker also belongs to the lid and therefore moves with it.
 for ob in list(current.objects):
  if ob.name.startswith('Fixed reading marker') and ob.parent==r:
   reparent_local(ob,cover)
catch=movable('WordBox_Catch',(0,-.16,.12),'hinge','X',70);box('Word latch',(0,0,0),(.045,.016,.06),brass,par=catch)
empty('Reward_MoonDisc',(0,0,.05));empty('Reward_BlockMa',(.12,0,.05));block('码',(.33,-.04,0),5)
plaque('05 · 归处',(0,-.28,.008));text('Word puzzle caption','四个字母，一个去处',(0,-.33,.01),.020,ivory)
# P06 moon flip ring.
r=collection('P06_MoonMechanism',(-.47,.15,0))
cyl('Moon wooden housing',(0,0,.045),.265,.09,wood,n=64);cyl('Moon dark face',(0,0,.094),.247,.012,dark,n=64)
cyl('Missing center recess',(0,0,.104),.077,.009,ink);empty('MoonDiscSocket',(0,0,.112),kind='install');box('Keyed central notch',(0,.075,.108),(.022,.022,.009),brass,.002)
for i in range(6):
 a=math.pi/2-i*math.pi/3;x=math.cos(a)*.166;y=math.sin(a)*.166
 pivot=movable('MoonTile_%d'%i,(x,y,.126),'flip','X',180);pivot['toggles_neighbors']='%d,%d'%((i+1)%6,(i+3)%6);pivot['initial_bright']=i in [0,1,3]
 cyl('Moon tile body',(0,0,0),.061,.021,brass,par=pivot)
 cyl('Moon bright side',(0,0,.012),.055,.004,ivory,par=pivot)
 cyl('Moon dark side',(0,0,-.012),.055,.004,ink,par=pivot)
 for sign in [-1,1]:cyl('Moon axle',(x+sign*.064,y,.126),.006,.025,brass,(0,math.pi/2,0))
 if i not in [0,1,3]:pivot.rotation_euler.x=math.pi
center_piece=empty('Pickup_MoonDisc',(.36,.02,.02),kind='pickup');center_piece['item_id']='moon_disc';cyl('Moon centerpiece',(0,0,.01),.071,.022,wood,par=center_piece);cyl('Moon medallion',(0,0,.024),.056,.005,ivory,par=center_piece);box('Centerpiece notch',(0,.068,.01),(.018,.018,.02),dark,.002,center_piece)
reset=movable('Moon_Reset',(.20,-.22,.06),'press','Z',-.008);cyl('Reset button',(0,0,0),.025,.018,brass,par=reset);text('Reset arrow','↺',(0,0,.011),.034,ink,par=reset)
reward_drawer('MoonRewardDrawer',(0,-.035,.008),.28,.32,.055);empty('Reward_SmallBoat',(0,0,.03));empty('Reward_TrayHandle',(.08,0,.03))
plaque('06 · 满月',(0,-.35,.008))
# P07 ordering tray and four pickup objects.
r=collection('P07_ObjectSequence',(.70,.15,0))
tray('Sequence cabinet',(0,0,0),(.70,.28,.14));board=movable('Sequence_SlidingTray',(0,0,.13),'slide','-Y',.20);box('Sequence surface',(0,0,0),(.66,.25,.018),wood,par=board)
for i in range(4):
 x=-.24+i*.16;cyl('Object well',(x,0,.012),.064,.009,ink,par=board);empty('ObjectSocket_%d'%i,(x,0,.025),board,'install');text('Sequence slot number',str(i+1),(x,-.09,.014),.019,ivory,par=board)
text('Sequence reading arrow','→',(-.32,0,.15),.04,ivory)
for x in [-.05,.05]:cyl('Handle socket',(x,-.142,.12),.008,.008,ink,(math.pi/2,0,0))
empty('TrayHandleSocket',(0,-.16,.12),kind='install')
handle=empty('Pickup_TrayHandle',(.42,-.08,.03),kind='pickup');handle['item_id']='tray_handle';rod('Handle grip',(-.055,0,.01),(.055,0,.01),.008,brass,handle)
for x in [-.055,.055]:rod('Handle peg',(x,0,.01),(x,.03,.01),.006,brass,handle)
def token(name,loc):
 p=empty('Pickup_'+name,loc,kind='pickup');p['item_id']=name;cyl('Token round plinth',(0,0,.009),.05,.018,wood,par=p);return p
for i,name in enumerate(['Pine','Stone','Boat','Moon']):
 p=token(name,(-.24+i*.16,-.25,.005));p['sequence_solution_index']=i
 if name=='Pine':
  rod('Pine twig',(0,0,.02),(0,0,.16),.004,dark,p)
  for j in range(5):
   z=.05+j*.021
   for sign in [-1,1]:
    rod('Pine branch',(0,0,z),(sign*.032,0,z+.017),.0025,dark,p)
    for k in range(3):rod('Pine needle',(sign*(.01+k*.008),0,z+.005),(sign*(.017+k*.008),.012,z+.025),.0015,green,p)
 elif name=='Stone':
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(0,0,.045));o=put(bpy.context.object,'Smooth river stone',ivory,p);o.scale=(.037,.028,.028)
 elif name=='Boat':
  box('Small boat hull',(0,0,.038),(.075,.035,.03),wood,.012,p);rod('Boat mast',(0,0,.045),(0,0,.13),.003,brass,p)
  me=bpy.data.meshes.new('Triangular sail');me.from_pydata([(0,0,.065),(0,0,.127),(.043,0,.065)],[],[(0,1,2)]);o=bpy.data.objects.new('Boat canvas sail',me);current.objects.link(o);o.parent=p;me.materials.append(paper);so=o.modifiers.new('Sail thickness','SOLIDIFY');so.thickness=.001
 else:
  cyl('Moon token disc',(0,0,.075),.039,.010,brass,(math.pi/2,0,0),p);rod('Moon support',(0,0,.02),(0,0,.05),.004,brass,p)
reward_drawer('SequenceRewardDrawer',(0,0,.009),.60,.23,.08);empty('Reward_FinalBox',(0,0,.03));block('头',(.43,.07,0),7)
plaque('07 · 顺序',(0,-.37,.008))
# P08 final box with four individual removable characters.
r=collection('P08_FinalWoodBox',(1.91,.15,0))
tray('Final chest',(0,0,0),(.47,.32,.16));cover=lid('FinalBox_Lid',(0,.16,.16),.47,.32,0)
for i in range(4):
 x=-.15+i*.10;box('WoodBlock socket',(x,-.16,.030),(.089,.089,.015),dark,.003,cover)
 slot=movable('LetterSocket_%d'%i,(x,-.16,.04),'slide','-Z',.025,cover);slot['expected_character']='第七码头'[i];empty('Install_WoodBlock_'+str(i),(0,0,.015),slot,'install')
text('Destination caption','目的地',(0,-.265,.027),.029,ivory,par=cover)
for x in [-.19,.19]:box('Chest corner strap',(x,0,.17),(.035,.31,.012),brass)
catch=movable('FinalBox_Latch',(0,-.17,.14),'hinge','X',80);box('Final latch',(0,0,0),(.04,.018,.06),brass,par=catch)
exitkey=key('Pickup_ExitKey',(.32,0,.01));exitkey['item_id']='exit_key';exitkey['reward_stage']=8
empty('Reward_ExitKey',(0,0,.04));empty('Reward_LastLetter',(0,.04,.025))
plaque('08 · 第七码头',(0,-.31,.008),(.34,.065))
# P09 all printed clue carriers, ready for replacing text.
r=collection('P09_ClueCards',(-1.48,-1.15,0))
cards=[('START','123',(-.25,.10,0)),('WORD','LOVE',(.0,.10,0)),('COMPASS','日：东\n屋：北\n月：西',(.25,.10,0)),('SEQUENCE','四行的开头\n是回家的顺序',(-.25,-.15,0)),('TICKET','船票\n目的地：＿＿＿＿',(0,-.15,0)),('LETTER','把门留给明天\n我们约好的地方\n你现在已经找齐',(.25,-.15,0))]
for name,body,loc in cards:
 p=empty('ClueCard_'+name,loc,kind='inspect');box('Paper card',(0,0,.003),(.21,.18,.004),paper,.002,p);text('Card typography',body,(0,0,.006),.019,ink,par=p)
plaque('09 · 线索纸件',(0,-.32,.008),(.34,.065))
# Studio only, never exported.
current=bpy.data.collections.new('STUDIO_DoNotExport');scene.collection.children.link(current);parent=None
def studio_object(o,name):
 o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 current.objects.link(o);return o
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.035));o=studio_object(bpy.context.object,'Studio floor');o.data.materials.append(material('StudioGreen',(.035,.055,.047)))
def camera(name,loc,target,ortho):
 d=bpy.data.cameras.new(name);o=bpy.data.objects.new(name,d);current.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=ortho;d.clip_end=300;return o
scene.camera=camera('CAM_AssetOverview',(4,-7,10),(.2,.5,0),6.4)
for idx,r in enumerate(roots):camera('CAM_'+r.name,tuple(r.location+Vector((.85,-1.2,1.5))),tuple(r.location+Vector((0,0,.12))),1.25 if idx not in [1,8] else 1.45)
def area(name,loc,target,power,size,color):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color;o=bpy.data.objects.new(name,d);current.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
area('Studio key',(0,-3,7),(0,0,0),1600,6,(1,.86,.68));area('Studio fill',(-4,1,4),(0,0,0),1000,5,(.70,.82,1));area('Studio rim',(4,4,5),(0,0,0),1800,4,(1,.82,.55))
world=bpy.data.worlds.new('Prop studio world');world.use_nodes=True;bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.16,.19,.17,1);bg.inputs[1].default_value=.45;scene.world=world
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.render.resolution_x=1800;scene.render.resolution_y=1200;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
scene.render.filepath=BASE+'Renders/PropLibrary.png'
scene['asset_roots']='|'.join(r.name for r in roots);scene['README']='Nine asset groups. Hinges and tiles use local pivots. Text is real geometry at FBX export. Materials use local PNG, no Blender runtime required. Display positions only; individual FBX roots reset to zero.'
for screen in bpy.data.screens:
 for a in screen.areas:
  if a.type=='VIEW_3D':a.spaces.active.region_3d.view_perspective='CAMERA';a.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=BASE+'Poetry_PuzzleProps.blend')
print('Created puzzle library:',len(scene.objects),'objects;',len(roots),'asset groups')
