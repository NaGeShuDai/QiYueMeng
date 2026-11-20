import socket, struct, json, sys
from pathlib import Path
def call(command, params):
    with socket.create_connection(('127.0.0.1',9876),timeout=180) as s:
        def recv(n):
            data=b''
            while len(data)<n:
                b=s.recv(n-len(data))
                if not b: raise RuntimeError('Connection closed')
                data+=b
            return data
        data=json.dumps({'command':command,'params':params}).encode()
        s.sendall(struct.pack('>I',len(data))); s.sendall(data)
        return json.loads(recv(struct.unpack('>I',recv(4))[0]))
if __name__=='__main__':
    result=call('execute_code',{'code':Path(sys.argv[1]).read_text(encoding='utf-8-sig')}) if len(sys.argv)>1 else call('get_scene_info',{})
    print(json.dumps(result,ensure_ascii=True,indent=2))
