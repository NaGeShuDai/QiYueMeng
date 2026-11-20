import socket,struct,json,sys,time
def call(kind,params):
 for attempt in range(15):
  try:
   connection=socket.create_connection(('127.0.0.1',6400),timeout=90);break
  except ConnectionRefusedError:
   if attempt==14:raise
   time.sleep(1)
 with connection as s:
  def read(n):
   b=b''
   while len(b)<n:
    t=s.recv(n-len(b))
    if not t:raise RuntimeError('Unity connection closed')
    b+=t
   return b
  greeting=b''
  while not greeting.endswith(b'\n'):greeting+=read(1)
  payload=json.dumps({'type':kind,'params':params}).encode()
  s.sendall(struct.pack('>Q',len(payload))+payload)
  return json.loads(read(struct.unpack('>Q',read(8))[0]))
if __name__=='__main__':
 print(json.dumps(call(sys.argv[1],json.loads(sys.argv[2]) if len(sys.argv)>2 else {}),ensure_ascii=True,indent=2))
