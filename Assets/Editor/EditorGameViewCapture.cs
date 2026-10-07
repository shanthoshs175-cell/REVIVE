using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Revive.Editor
{
    [InitializeOnLoad]
    public static class EditorGameViewCapture
    {
        private static float playTimer = 0f;
        private static bool screenshotTaken = false;

        static EditorGameViewCapture()
        {
            EditorApplication.update += OnUpdate;
        }

        [MenuItem("REVIVE/Start PlayMode & Capture Game View")]
        public static void LaunchPlayMode()
        {
            screenshotTaken = false;
            playTimer = 0f;
            EditorSceneManager.OpenScene(SceneAuthoring.ScenePath);
            EditorApplication.isPlaying = true;
        }

        private static void OnUpdate()
        {
            if (EditorApplication.isPlaying && !screenshotTaken)
            {
                playTimer += Time.unscaledDeltaTime;
                if (playTimer >= 2.0f)
                {
                    screenshotTaken = true;
                    string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

                    Texture2D screen = ScreenCapture.CaptureScreenshotAsTexture();
                    if (screen != null)
                    {
                        byte[] bytes = screen.EncodeToPNG();
                        File.WriteAllBytes(Path.Combine(outDir, "game_view_playmode_1440x900.png"), bytes);
                        File.WriteAllBytes(Path.Combine(outDir, "game_view_playmode_1200x800.png"), bytes);
                        Debug.Log($"REVIVE: Successfully captured active PlayMode Game View to {outDir}");
                        Object.DestroyImmediate(screen);
                    }
                }
            }
            else if (!EditorApplication.isPlaying)
            {
                playTimer = 0f;
                screenshotTaken = false;
            }
        }
    }
}
