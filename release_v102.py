import runpy,sys,urllib.parse,hashlib,json,subprocess
from pathlib import Path
sys.argv=['publish_github.py','noop']
s=runpy.run_path('E:/ProgramFiles/_Project/Puzzle_LocalArchive_20261105/ProcessFiles/publish_github.py');api=s['api'];base=s['base']
commit=subprocess.check_output(['git','-C','Publish/QiYueMeng','rev-parse','HEAD'],text=True).strip()
r=api(base+'/releases',{'tag_name':'v1.0.2','target_commitish':commit,'name':'七月梦 v1.0.2 · 新图标与退出按钮','body':'新增 APIClaude 生成的木门月光码头图标并嵌入 Windows EXE；通关按钮改为“去往目的地”；右上角新增“退出游戏”，退出前保存进度。已验证程序图标及独立 EXE 实际点击退出。保留 v1.0.1 的指南针答案和结尾网页修复。完整解压后运行七月梦.exe，七月梦.pdf已随包附送。','draft':True,'prerelease':False})
Path('UnityValidation/release_v102.json').write_text(json.dumps({'id':r['id']}))
for path,name,label in [('Release/七月梦-Windows-x64-v1.0.2.zip','QiYueMeng-Windows-x64-v1.0.2.zip','七月梦 v1.0.2 · Windows 完整版'),('七月梦.pdf','QiYueMeng.pdf','七月梦.pdf')]:
 p=Path(path);a=api(r['upload_url'].split('{')[0]+'?'+urllib.parse.urlencode({'name':name,'label':label}),method='POST',raw=p.read_bytes());assert a['size']==p.stat().st_size;assert a['digest']=='sha256:'+hashlib.sha256(p.read_bytes()).hexdigest();print('Verified upload',name,flush=True)
api(base+'/releases/'+str(r['id']),{'draft':False,'make_latest':'true'},method='PATCH')
latest=api(base+'/releases/latest');assert latest['tag_name']=='v1.0.2' and not latest['draft'];print(latest['html_url'],flush=True)
