using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
public sealed class PoetryClueValidation:MonoBehaviour {
 public void Run(){StartCoroutine(Check());}
 IEnumerator Check(){var g=FindFirstObjectByType<PoetryPuzzleGame>();string dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");foreach(var target in g.view.targets.Where(t=>t&&t.label.StartsWith("梦中梦"))){g.view.ReturnToOverview();for(int a=0;a<360;a+=45){g.view.SetOrbit(a);g.view.Snap();if(target.Available)break;}g.view.Focus(target);g.view.Snap();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/"+(target.label.Contains("II")?"poem_II":"poem_I")+".png");yield return null;}
 string snapshot=JsonUtility.ToJson(g.State);bool persist=g.persist;g.persist=false;g.State.solved[3]=true;g.State.solved[4]=false;g.Refresh();g.view.ReturnToOverview();for(int a=0;a<360;a+=45){g.view.SetOrbit(a);g.view.Snap();if(g.stations[4].focus.Available)break;}g.view.Focus(g.stations[4].focus);g.view.Snap();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/love_clues_closed.png");yield return null;
 bool visibleBefore=g.stations[4].model.Find("Poetry LOVE clues").gameObject.activeSelf;
 for(int i=0;i<4;i++)g.State.letters[i]=0;g.State.Check(4);g.Refresh();yield return null;
 bool hiddenAfter=!g.stations[4].model.Find("Poetry LOVE clues").gameObject.activeSelf;
 yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/love_clues_solved.png");yield return null;
 File.WriteAllText(dir+"/clue_presentation_validation.txt",(visibleBefore&&hiddenAfter?"PASS":"FAIL")+" LOVE clues visible before solve and hidden after solve\n"+(g.stations[2].model.GetComponentsInChildren<TextMesh>().All(t=>!t.name.StartsWith("Direction "))?"PASS":"FAIL")+" no compass direction words");
 g.RestoreSnapshot(snapshot);g.persist=persist;
 var checks=new System.Collections.Generic.List<string>();for(int version=1;version<=3;version++){var s=new PoetryPuzzleState{version=version};s.solved[0]=true;s.inventory.Add("winding_key");string reward=s.RewardBlock(2);s.Upgrade();checks.Add((s.Valid()&&s.solved[0]&&s.Has("winding_key")&&s.RewardBlock(2)==reward&&s.digits.SequenceEqual(new[]{5,6,8})?"PASS":"FAIL")+" migration v"+version);}var n=new PoetryPuzzleState();n.digits=new[]{1,2,3};checks.Add(!n.Check(0)?"PASS rejects old 123":"FAIL old code");n.digits=new[]{5,6,8};checks.Add(n.Check(0)?"PASS accepts 568":"FAIL new code");File.WriteAllLines(dir+"/poetry_clue_validation.txt",checks);g.view.ReturnToOverview();Debug.Log("POETRY_CLUE_VALIDATION_DONE");}
}
