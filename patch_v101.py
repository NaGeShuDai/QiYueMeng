from pathlib import Path
p=Path('Puzzle/Assets/PoetryRoom/Scripts/PoetryPuzzleState.cs');s=p.read_text(encoding='utf-8-sig').replace('public int version=4','public int version=5').replace('version<=4','version<=5').replace('new[]{1,5,3}','new[]{1,3,3}').replace('if(version>=4)return;','if(version>=5)return;if(version==4){if(solved[2])compass=new[]{1,3,3};version=5;return;}').replace('version=4;}','version=5;}');p.write_text(s,encoding='utf-8')
p=Path('Puzzle/Assets/PoetryRoom/Scripts/PoetryPhysicalValidation.cs');s=p.read_text(encoding='utf-8-sig').replace('new[]{1,5,3}','new[]{1,3,3}');p.write_text(s,encoding='utf-8')
p=Path('Puzzle/Assets/PoetryRoom/Scripts/EndingPortal.cs');s=p.read_text(encoding='utf-8-sig').replace('    public int LaunchCount', '    public string LastBrowserError { get; private set; }\n    public string LastOpenedPath { get; private set; }\n    public int LaunchCount');a=s.index('        if (!File.Exists(WebsitePath))');b=s.index('\n    }',a);s=s[:a]+'''        LastBrowserError = null;
        string path = Path.GetFullPath(WebsitePath);
        if (!File.Exists(path)) {
            LastBrowserError = "Missing website: " + path;
            game.Tell("网页文件不完整，请完整解压游戏后重试。");
            Debug.LogError(LastBrowserError);return;
        }
        try {
            if (!suppressBrowserForValidation) {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                // Let the Windows shell open the local HTML path directly, including Chinese and spaces.
                using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                    FileName = path, UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(path)
                })) { }
#else
                Application.OpenURL(new Uri(path).AbsoluteUri);
#endif
            }
            LastOpenedPath = path;LaunchCount++;
            Debug.Log("ENDING_BROWSER_OPEN_REQUESTED " + path);
        } catch (Exception e) {
            LastBrowserError = e.Message;
            game.Tell("网页未能打开，可在游戏目录双击“打开纪念网页”。");
            Debug.LogError("Opening ending website failed: " + e);
        }''' + s[b:];p.write_text(s,encoding='utf-8')
p=Path('Puzzle/Assets/Editor/PoetryReleaseBuild.cs');s=p.read_text(encoding='utf-8-sig').replace('  PlayerSettings.productName=', '  PlayerSettings.bundleVersion="1.0.1";\n  PlayerSettings.productName=');s=s.replace('  if(report.summary.result!=BuildResult.Succeeded)', '  File.WriteAllText(Path.Combine(dir,"打开纪念网页.url"),"[InternetShortcut]\\r\\nURL="+new Uri(Path.Combine(dir,"七月梦_Data/StreamingAssets/OurStory/index.html")).AbsoluteUri+"\\r\\n");\n  if(report.summary.result!=BuildResult.Succeeded)');# use relocatable bat instead of absolute url
s=s.replace('File.WriteAllText(Path.Combine(dir,"打开纪念网页.url"),"[InternetShortcut]\\r\\nURL="+new Uri(Path.Combine(dir,"七月梦_Data/StreamingAssets/OurStory/index.html")).AbsoluteUri+"\\r\\n");','File.WriteAllText(Path.Combine(dir,"打开纪念网页.bat"),"@echo off\\r\\nchcp 65001 >nul\\r\\nstart \\\"\\\" \\\"%~dp0七月梦_Data\\\\StreamingAssets\\\\OurStory\\\\index.html\\\"\\r\\n",new System.Text.UTF8Encoding(false));')
p.write_text(s,encoding='utf-8')
