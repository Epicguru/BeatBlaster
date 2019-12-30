
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public static int FPS = 0;

    private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    private int frames;

    private void Awake()
    {
        watch.Start();
    }

    private void Update()
    {
        frames++;
    }

    private void OnGUI()
    {
        if(watch.Elapsed.TotalSeconds >= 1f)
        {
            watch.Restart();
            FPS = frames;
            frames = 0;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, Screen.height - 20));
        GUILayout.Label($"Fps: {FPS}");
        GUILayout.EndArea();
    }
}
