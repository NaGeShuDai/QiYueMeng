from pathlib import Path
import math,random,struct,zlib
root=Path('PuzzleProps/Textures');root.mkdir(parents=True,exist_ok=True)
def png(name,width,height,pixel):
 def chunk(t,d):return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
 raw=bytearray()
 for y in range(height):
  raw.append(0)
  for x in range(width):raw.extend(pixel(x,y))
 data=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',width,height,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(bytes(raw),7))+chunk(b'IEND',b'')
 (root/name).write_bytes(data)
random.seed(65)
def wood(x,y):
 v=.50+.16*math.sin(x*.16+2*math.sin(y*.008)+math.sin(x*.025+y*.004))+.08*math.sin(x*.67+math.sin(y*.013))+.045*math.sin(x*2.7+y*.006)+random.uniform(-.025,.025)
 return tuple(int(max(0,min(255,a+b*v))) for a,b in [(45,96),(20,65),(9,35)])
png('AntiqueWalnut_BaseColor.png',1024,1024,wood)
print('Created 1024px standalone wood texture')
