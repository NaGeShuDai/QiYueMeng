using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class PoetryReleaseBuild {
 [MenuItem("Tools/Poetry Room/Build Windows Release")]
 public static void Build(){
  if(EditorApplication.isPlaying)throw new Exception("Stop Play before building");
  string dir=Path.GetFullPath(Application.dataPath+"/../../Release/七月梦");Directory.CreateDirectory(dir);
  PlayerSettings.bundleVersion="1.0.3";
  PlayerSettings.productName="七月梦";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;
  var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PoetryRoom/Branding/QiYueMengIcon.png");
  if(!icon)throw new Exception("Missing game icon");
  PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
  var sizes=PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone,IconKind.Any);
  if(sizes.Length>0)PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone,System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Repeat(icon,sizes.Length)),IconKind.Any);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/PoetryRoom/Scenes/PoetryBedroom.unity"},locationPathName=dir+"/七月梦.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Directory.CreateDirectory(Path.GetFullPath(Application.dataPath+"/../../UnityValidation"));
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/release_build.txt"),report.summary.result+" errors="+report.summary.totalErrors+" bytes="+report.summary.totalSize);
  string launcher="@echo off\r\nfor /d %%D in (\"%~dp0*_Data\") do if exist \"%%~fD\\StreamingAssets\\OurStory\\launch.ps1\" (\r\n powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"%%~fD\\StreamingAssets\\OurStory\\launch.ps1\"\r\n exit /b\r\n)\r\necho Website files not found.\r\npause\r\n";
  File.WriteAllText(Path.Combine(dir,"webstart.bat"),launcher,System.Text.Encoding.ASCII);
  File.WriteAllText(Path.Combine(dir,"打开纪念网页.bat"),"@echo off\r\ncall \"%~dp0webstart.bat\"\r\n",System.Text.Encoding.ASCII);
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Release build failed");
 }
}
