using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;

[InitializeOnLoad]
public static class PuzzleMcpConnect
{
    static double next;
    static bool connecting;
    static PuzzleMcpConnect()
    {
        EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", false);
        EditorPrefs.SetString("MCPForUnity.HttpUrl", "http://127.0.0.1:8080");
        EditorApplication.update += Tick;
    }
    static async void Tick()
    {
        if (connecting || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
        next=EditorApplication.timeSinceStartup+30;
        if (SessionState.GetBool("Puzzle.McpConnected",false)) return;
        connecting=true;
        try { if (await MCPServiceLocator.TransportManager.StartAsync(TransportMode.Stdio)) SessionState.SetBool("Puzzle.McpConnected",true); }
        catch (System.Exception e) { Debug.Log("Puzzle MCP connecting: "+e.Message); }
        finally { connecting=false; }
    }
}
