from pathlib import Path
p=Path('Publish/QiYueMeng/README.md');s=p.read_text(encoding='utf-8');s=s.replace('## 本次修复\n\n扩大总览滚轮缩放范围，增加聚焦后的滚轮缩放；缩放没有惯性。','## v1.0.1 更新\n\n- 第二枚指南针的正确方向改为东南，旧存档保留进度。\n- Windows 结尾网页改用系统文件打开机制，支持中文及带空格的解压路径。\n- 打开失败时显示提示，并附送“打开纪念网页.bat”供手动打开。\n- 保留 v1.0.0 的总览与聚焦滚轮缩放修复。');p.write_text(s,encoding='utf-8')
p=Path('诗集谜题对应方案.md');s=p.read_text(encoding='utf-8-sig').replace('怀疑西南','怀疑东南');p.write_text(s,encoding='utf-8')
