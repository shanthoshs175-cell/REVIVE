using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Revive.Game;
using Revive.Scenarios;
using Revive.Presentation;

namespace Revive.Editor
{
    public static class VisualVerificationTool
    {
        [MenuItem("REVIVE/Run Verification Screenshots")]
        public static void CaptureScreenshots()
        {
            EditorSceneManager.OpenScene(SceneAuthoring.ScenePath);

            // Ensure rendering camera and scene setup are ready
            var cameraObj = GameObject.Find("Presentation Camera");
            Camera camera = cameraObj != null ? cameraObj.GetComponent<Camera>() : Camera.main;

            string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            // Render 1440x900
            RenderToPNG(camera, 1440, 900, Path.Combine(outDir, "game_view_1440x900.png"));
            // Render 1200x800
            RenderToPNG(camera, 1200, 800, Path.Combine(outDir, "game_view_1200x800.png"));

            Debug.Log($"REVIVE Verification: Screenshots rendered successfully to {outDir}");
        }

        private static void RenderToPNG(Camera cam, int width, int height, string filePath)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            Object.DestroyImmediate(screenShot);
        }
    }
}
