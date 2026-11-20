using UnityEngine;
// Keep the existing menu entry connected to the current model-only suite.
public sealed class PoetryPuzzleValidation:MonoBehaviour {
 public void Run(){gameObject.AddComponent<PoetryPhysicalValidation>().Run();Destroy(this);}
}
