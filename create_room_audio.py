"""Original quiet music and interface tones, generated without external assets."""
import math, wave, struct
from pathlib import Path
RATE=22050
out=Path('Puzzle/Assets/PoetryRoom/Resources/RoomAudio');out.mkdir(parents=True,exist_ok=True)
def write(name,samples):
    peak=max(abs(x) for x in samples) or 1
    gain=min(1,.82/peak)
    with wave.open(str(out/(name+'.wav')),'wb') as f:
        f.setparams((1,2,RATE,0,'NONE','not compressed'))
        f.writeframes(b''.join(struct.pack('<h',int(max(-1,min(1,x*gain))*32767)) for x in samples))
def add(buf,start,freq,duration,amp):
    offset=int(start*RATE)
    for k in range(int(duration*RATE)):
        t=k/RATE
        env=(1-math.exp(-t*45))*math.exp(-t*2.3)*(min(1,(duration-t)*8))
        value=amp*env*(math.sin(2*math.pi*freq*t)+.22*math.sin(2*math.pi*freq*2*t)+.07*math.sin(2*math.pi*freq*3*t))
        if offset+k<len(buf):buf[offset+k]+=value
music=[0.]*(RATE*32)
for bar,chord in enumerate([(48,55,60,64),(45,52,57,60),(41,48,53,57),(43,50,55,59)]*2):
    for beat,index in enumerate([0,2,1,3]):add(music,bar*4+beat,440*2**((chord[index]-69)/12),2.6,.23)
for i in range(RATE):music[i]*=i/RATE;music[-i-1]*=i/RATE
write('background',music)
for name,notes in [('interact',[(0,740,.085,.32)]),('success',[(0,523,.55,.27),(.13,659,.55,.25),(.26,784,.7,.23)]),('error',[(0,220,.21,.25),(.12,185,.30,.22)])]:
    sound=[0.]*(RATE*2)
    for args in notes:add(sound,*args)
    end=int((max(t+d for t,_,d,_ in notes)+.02)*RATE)
    write(name,sound[:end])
print('Created four original WAV assets in',out)
