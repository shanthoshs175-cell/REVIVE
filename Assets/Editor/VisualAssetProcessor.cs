using System.IO;
using UnityEditor;
using UnityEngine;

namespace Revive.Editor
{
    public static class VisualAssetProcessor
    {
        public const string SourceDir = "Assets/Sprites/Source";
        public const string EnvDir = "Assets/Resources/Sprites/Environment";
        public const string CharDir = "Assets/Resources/Sprites/Characters";
        public const string FXDir = "Assets/Resources/Sprites/Effects";

        [MenuItem("REVIVE/Process Visual Assets")]
        public static void ProcessAssets()
        {
            EnsureDirectories();

            // 1. Process Environment BG
            string bgPath = Path.Combine(SourceDir, "bg.png");
            if (File.Exists(bgPath))
            {
                byte[] bgBytes = File.ReadAllBytes(bgPath);
                Texture2D bgTex = new Texture2D(2, 2);
                bgTex.LoadImage(bgBytes);
                SaveTextureAsSprite(bgTex, Path.Combine(EnvDir, "industrial_bg.png"), false);
                Object.DestroyImmediate(bgTex);
            }

            // 2. Process Player Poses (2x3 grid)
            string playerPath = Path.Combine(SourceDir, "player.png");
            if (File.Exists(playerPath))
            {
                byte[] pBytes = File.ReadAllBytes(playerPath);
                Texture2D pTex = new Texture2D(2, 2);
                pTex.LoadImage(pBytes);
                int w = pTex.width;
                int h = pTex.height;

                // 2 rows, 3 cols
                // Unity Texture coordinates: y=0 is bottom, y=h is top!
                // Top row in image is y from h/2 to h
                // Bottom row in image is y from 0 to h/2

                // Top Row (y from h/2 to h): Col 0 = Idle, Col 1 = Approach, Col 2 = Assess
                Texture2D idle = CropAndKeyWhite(pTex, 0, h / 2, w / 3, h / 2);
                Texture2D approach = CropAndKeyWhite(pTex, w / 3, h / 2, w / 3, h / 2);
                Texture2D assess = CropAndKeyWhite(pTex, 2 * w / 3, h / 2, w / 3, h / 2);

                // Bottom Row (y from 0 to h/2): Col 0 = Direct, Col 1 = Recoil, Col 2 = Call
                Texture2D direct = CropAndKeyWhite(pTex, 0, 0, w / 3, h / 2);
                Texture2D recoil = CropAndKeyWhite(pTex, w / 3, 0, w / 3, h / 2);
                Texture2D call = CropAndKeyWhite(pTex, 2 * w / 3, 0, w / 3, h / 2);

                SaveTextureAsSprite(idle, Path.Combine(CharDir, "player_idle.png"));
                SaveTextureAsSprite(approach, Path.Combine(CharDir, "player_approach.png"));
                SaveTextureAsSprite(assess, Path.Combine(CharDir, "player_assess.png"));
                SaveTextureAsSprite(direct, Path.Combine(CharDir, "player_direct.png"));
                SaveTextureAsSprite(recoil, Path.Combine(CharDir, "player_recoil.png"));
                SaveTextureAsSprite(call, Path.Combine(CharDir, "player_call.png"));

                // Also make reach (pointing/extending arm) & monitor (idle stance)
                SaveTextureAsSprite(direct, Path.Combine(CharDir, "player_reach.png"));
                SaveTextureAsSprite(idle, Path.Combine(CharDir, "player_monitor.png"));

                Object.DestroyImmediate(pTex);
                Object.DestroyImmediate(idle);
                Object.DestroyImmediate(approach);
                Object.DestroyImmediate(assess);
                Object.DestroyImmediate(direct);
                Object.DestroyImmediate(recoil);
                Object.DestroyImmediate(call);
            }

            // 3. Process Casualty and Bystander
            string cbPath = Path.Combine(SourceDir, "casualty_bystander.png");
            if (File.Exists(cbPath))
            {
                byte[] cbBytes = File.ReadAllBytes(cbPath);
                Texture2D cbTex = new Texture2D(2, 2);
                cbTex.LoadImage(cbBytes);
                int w = cbTex.width;
                int h = cbTex.height;

                // Left side = Casualty lying
                Texture2D casualty = CropAndKeyWhite(cbTex, 0, 0, (int)(w * 0.65f), h);
                // Right side = Bystander standing
                Texture2D bystander = CropAndKeyWhite(cbTex, (int)(w * 0.65f), 0, (int)(w * 0.35f), h);

                SaveTextureAsSprite(casualty, Path.Combine(CharDir, "casualty_lying.png"));
                SaveTextureAsSprite(bystander, Path.Combine(CharDir, "bystander_anxious.png"));

                Object.DestroyImmediate(cbTex);
                Object.DestroyImmediate(casualty);
                Object.DestroyImmediate(bystander);
            }

            // 4. Process Effects (2x2 grid, black background key)
            string fxPath = Path.Combine(SourceDir, "effects.png");
            if (File.Exists(fxPath))
            {
                byte[] fxBytes = File.ReadAllBytes(fxPath);
                Texture2D fxTex = new Texture2D(2, 2);
                fxTex.LoadImage(fxBytes);
                int w = fxTex.width;
                int h = fxTex.height;

                // Top-Left: Arc (x=0, y=h/2)
                Texture2D arc = CropAndKeyBlack(fxTex, 0, h / 2, w / 2, h / 2);
                // Top-Right: Hazard (x=w/2, y=h/2)
                Texture2D hazard = CropAndKeyBlack(fxTex, w / 2, h / 2, w / 2, h / 2);
                // Bottom-Left: Scan (x=0, y=0)
                Texture2D scan = CropAndKeyBlack(fxTex, 0, 0, w / 2, h / 2);
                // Bottom-Right: Call (x=w/2, y=0)
                Texture2D callIcon = CropAndKeyBlack(fxTex, w / 2, 0, w / 2, h / 2);

                SaveTextureAsSprite(arc, Path.Combine(FXDir, "electrical_arc.png"));
                SaveTextureAsSprite(hazard, Path.Combine(FXDir, "hazard_warning.png"));
                SaveTextureAsSprite(scan, Path.Combine(FXDir, "scan_reticle.png"));
                SaveTextureAsSprite(callIcon, Path.Combine(FXDir, "call_icon.png"));

                Object.DestroyImmediate(fxTex);
                Object.DestroyImmediate(arc);
                Object.DestroyImmediate(hazard);
                Object.DestroyImmediate(scan);
                Object.DestroyImmediate(callIcon);
            }

            AssetDatabase.Refresh();
            ConfigureImporterSettings();
            Debug.Log("REVIVE: Visual assets processed and configured successfully!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(EnvDir)) Directory.CreateDirectory(EnvDir);
            if (!Directory.Exists(CharDir)) Directory.CreateDirectory(CharDir);
            if (!Directory.Exists(FXDir)) Directory.CreateDirectory(FXDir);
        }

        private static Texture2D CropAndKeyWhite(Texture2D src, int x, int y, int width, int height)
        {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = src.GetPixels(x, y, width, height);

            // Trim transparent edges bounding box calculation
            int minX = width, maxX = 0, minY = height, maxY = 0;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                // White chroma key: r > 0.88, g > 0.88, b > 0.88
                float brightness = (c.r + c.g + c.b) / 3f;
                float colorDiff = Mathf.Max(Mathf.Abs(c.r - c.g), Mathf.Abs(c.g - c.b), Mathf.Abs(c.r - c.b));

                if (brightness > 0.88f && colorDiff < 0.08f)
                {
                    // Soft falloff for antialiased white edges
                    float alpha = Mathf.Clamp01((0.96f - brightness) / 0.08f);
                    pixels[i] = new Color(c.r, c.g, c.b, alpha);
                }
                else
                {
                    pixels[i] = new Color(c.r, c.g, c.b, 1f);
                }

                if (pixels[i].a > 0.05f)
                {
                    int px = i % width;
                    int py = i / width;
                    if (px < minX) minX = px;
                    if (px > maxX) maxX = px;
                    if (py < minY) minY = py;
                    if (py > maxY) maxY = py;
                }
            }

            if (minX <= maxX && minY <= maxY)
            {
                int cropW = maxX - minX + 1;
                int cropH = maxY - minY + 1;
                Texture2D cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
                Color[] cropPixels = new Color[cropW * cropH];
                for (int cy = 0; cy < cropH; cy++)
                {
                    for (int cx = 0; cx < cropW; cx++)
                    {
                        cropPixels[cy * cropW + cx] = pixels[(minY + cy) * width + (minX + cx)];
                    }
                }
                cropped.SetPixels(cropPixels);
                cropped.Apply();
                Object.DestroyImmediate(result);
                return cropped;
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private static Texture2D CropAndKeyBlack(Texture2D src, int x, int y, int width, int height)
        {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = src.GetPixels(x, y, width, height);

            int minX = width, maxX = 0, minY = height, maxY = 0;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float maxChannel = Mathf.Max(c.r, c.g, c.b);

                // Black chroma key based on brightness
                if (maxChannel < 0.15f)
                {
                    float alpha = Mathf.Clamp01((maxChannel - 0.02f) / 0.13f);
                    pixels[i] = new Color(c.r, c.g, c.b, alpha);
                }
                else
                {
                    pixels[i] = new Color(c.r, c.g, c.b, Mathf.Clamp01(maxChannel * 1.5f));
                }

                if (pixels[i].a > 0.05f)
                {
                    int px = i % width;
                    int py = i / width;
                    if (px < minX) minX = px;
                    if (px > maxX) maxX = px;
                    if (py < minY) minY = py;
                    if (py > maxY) maxY = py;
                }
            }

            if (minX <= maxX && minY <= maxY)
            {
                int cropW = maxX - minX + 1;
                int cropH = maxY - minY + 1;
                Texture2D cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
                Color[] cropPixels = new Color[cropW * cropH];
                for (int cy = 0; cy < cropH; cy++)
                {
                    for (int cx = 0; cx < cropW; cx++)
                    {
                        cropPixels[cy * cropW + cx] = pixels[(minY + cy) * width + (minX + cx)];
                    }
                }
                cropped.SetPixels(cropPixels);
                cropped.Apply();
                Object.DestroyImmediate(result);
                return cropped;
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private static void SaveTextureAsSprite(Texture2D tex, string path, bool hasAlpha = true)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        private static void ConfigureImporterSettings()
        {
            string[] dirs = { EnvDir, CharDir, FXDir };
            foreach (var dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "*.png");
                foreach (var file in files)
                {
                    AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
                    TextureImporter importer = AssetImporter.GetAtPath(file) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}
