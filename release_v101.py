import runpy,sys,urllib.parse,hashlib,json,subprocess
from pathlib import Path
sys.argv=['publish_github.py','noop']
s=runpy.run_path('E:/ProgramFiles/_Project/Puzzle_LocalArchive_20261105/ProcessFiles/publish_github.py');api=s['api'];base=s['base']
commit=subprocess.check_output(['git','-C','Publish/QiYueMeng','rev-parse','HEAD'],text=True).strip()
r=api(base+'/releases',{'tag_name':'v1.0.1','target_commitish':commit,'name':'七月梦 v1.0.1 · 指南针与结尾网页修复','body':'第二枚指南针答案改为东南；Windows 结尾网页改由系统直接打开本地 HTML，支持中文及空格路径。保留旧存档进度。已在独立 EXE 中使用最终钥匙开门并确认浏览器打开结尾页面。附带打开纪念网页.bat备用入口。请完整解压运行，配套七月梦.pdf已随包附送。','draft':True,'prerelease':False})
Path('UnityValidation/release_v101.json').write_text(json.dumps({'id':r['id']}))
for path,name,label in [('Release/七月梦-Windows-x64-v1.0.1.zip','QiYueMeng-Windows-x64-v1.0.1.zip','七月梦 v1.0.1 · Windows 完整版'),('七月梦.pdf','QiYueMeng.pdf','七月梦.pdf')]:
 p=Path(path);a=api(r['upload_url'].split('{')[0]+'?'+urllib.parse.urlencode({'name':name,'label':label}),method='POST',raw=p.read_bytes());assert a['size']==p.stat().st_size;assert a['digest']=='sha256:'+hashlib.sha256(p.read_bytes()).hexdigest();print('Verified upload',name,flush=True)
api(base+'/releases/'+str(r['id']),{'draft':False,'make_latest':'true'},method='PATCH')
latest=api(base+'/releases/latest');assert latest['tag_name']=='v1.0.1' and not latest['draft'];print(latest['html_url'],flush=True)
