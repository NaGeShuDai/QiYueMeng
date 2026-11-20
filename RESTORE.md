# 开发源码恢复（2026-11-20）

## 恢复

```powershell
git clone https://github.com/NaGeShuDai/QiYueMeng.git
cd QiYueMeng
python verify_backup.py
```

使用 Unity Hub 安装 Unity **6000.6.6f1**（Windows Build Support / IL2CPP），打开仓库下的 `Puzzle` 文件夹，等待 Unity 下载 Packages 中锁定的注册表依赖并重新导入资源。打开 `Assets/PoetryRoom/Scenes/PoetryBedroom.unity`，运行或使用 `Tools > Poetry Room > Build Windows Release` 构建。联网获取 Unity 及其注册表依赖仍然必要，Library 缓存不是离线安装包。

当前本地源码的构建版本为 1.0.3，仓库此前已发布的 1.0.2 安装包不等于本次源码快照。本次仅做源码恢复校验，没有重新完成 Unity 编译或游戏通关验收。

## 包含

- `Puzzle/Assets`：当前游戏脚本、场景（含保留的旧场景）、模型、材质、音频、网页程序及对应 meta GUID。
- `Puzzle/Packages`、`ProjectSettings`、`LocalPackages`：当前配置、依赖锁定和本地 Unity MCP 包；其连接脚本使用本机回环地址。
- `Poetry_Escape_Room.blend`、`PuzzleProps`、`Unity_Export`：可编辑 Blender 工程、FBX 和贴图。
- 根目录 Python 脚本：建模、导出、验证和旧版本制作脚本。部分含原电脑绝对路径；重新使用前按新电脑修改，正常打开 Unity 不需要运行这些脚本。
- `output/imagegen`：图标制作原图；最终诗集 PDF 和谜题设计说明保留。

## 明确排除

按所有者要求，不备份 `_res` 原始私人照片、网页 data 中的照片/预览/相册文字/重要日/旧备份，以及 content.js 私人称呼和信件；仅提供空白数据模板。删除本地文件后，无法从此仓库恢复这些个人内容。

不包含 Library、Temp、Logs、UserSettings、IDE 生成文件、构建产物、重复压缩包、过程缓存、第三方工具安装目录和 `.codex-poetry` 诗集排版过程目录。最终诗集 PDF 已保留；此备份用于游戏二次开发，不承诺恢复旧诗集排版工作流。原有 Git 提交历史保留，当前空白模板不意味着改写了历史版本。

## 网页

安装 Python 3 后运行 `Puzzle/Assets/StreamingAssets/OurStory/打开纪念网页.bat`，或在网站目录执行 `python server.py --port 18777`，访问本机 18777 端口。网站程序保留管理、导入导出及幻灯片功能，初始相册为空。

## 完整性

`BACKUP_MANIFEST.json` 记录此快照全部受跟踪文件（清单自身除外）的大小和 SHA-256。`python verify_backup.py` 可逐文件校验。普通 Git 文件保存，无 Git LFS 或子模块还原步骤。`UnityValidation` 构建日志目录现由构建脚本自动创建，适用于新克隆。
