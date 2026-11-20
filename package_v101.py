from pathlib import Path
import zipfile,hashlib
root=Path('Release/七月梦');out=Path('Release/七月梦-Windows-x64-v1.0.1.zip')
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in root.rglob('*'):
  if p.is_file() and not any('BackUpThisFolder' in part for part in p.parts):z.write(p,p.relative_to(root.parent))
print('bytes='+str(out.stat().st_size),'sha256='+hashlib.sha256(out.read_bytes()).hexdigest())
with zipfile.ZipFile(out) as z:
 assert z.testzip() is None
 assert '七月梦/打开纪念网页.bat' in z.namelist()
 assert '七月梦/七月梦.pdf' in z.namelist()
print('ZIP verified')
