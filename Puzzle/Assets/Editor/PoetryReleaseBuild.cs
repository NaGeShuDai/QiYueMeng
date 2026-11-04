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
  PlayerSettings.bundleVersion="1.0.1";
  PlayerSettings.productName="七月梦";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/PoetryRoom/Scenes/PoetryBedroom.unity"},locationPathName=dir+"/七月梦.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/release_build.txt"),report.summary.result+" errors="+report.summary.totalErrors+" bytes="+report.summary.totalSize);
  File.WriteAllText(Path.Combine(dir,"打开纪念网页.bat"),"@echo off\r\nchcp 65001 >nul\r\nstart \"\" \"%~dp0七月梦_Data\\StreamingAssets\\OurStory\\index.html\"\r\n",new System.Text.UTF8Encoding(false));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Release build failed");
 }
}
