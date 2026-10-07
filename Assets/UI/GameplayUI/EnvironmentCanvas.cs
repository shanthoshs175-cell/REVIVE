using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Revive.Scenarios;

namespace Revive.Presentation
{
    /// <summary>
    /// High-quality 2D/2.5D Layered Environment Canvas.
    /// Renders detailed environment, human-proportioned character sprites, and visual effects.
    /// Presentation only: no medical or decision logic.
    /// </summary>
    public sealed class EnvironmentCanvas : VisualElement
    {
        public float Incident { get; set; }
        public float Reaction { get; set; }
        public bool Warning { get; set; }
        public bool ReducedMotion { get; set; }
        public WorldResponse? Response { get; set; }
        public float ResponseProgress { get; set; }
        public bool AssessmentVisible { get; set; }
        public bool CommunicationVisible { get; set; }

        private readonly VisualElement bgLayer;
        private readonly VisualElement casualtyElement;
        private readonly VisualElement bystanderElement;
        private readonly VisualElement playerElement;
        private readonly VisualElement arcElement;
        private readonly VisualElement hazardIconElement;
        private readonly VisualElement scanElement;
        private readonly VisualElement callElement;
        private readonly VisualElement warningVignette;

        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public EnvironmentCanvas()
        {
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);
            style.overflow = Overflow.Hidden;
            pickingMode = PickingMode.Ignore;

            LoadSprites();

            // 1. Background Layer
            bgLayer = CreateLayer("bg-layer");
            bgLayer.style.left = 0;
            bgLayer.style.top = 0;
            bgLayer.style.width = Length.Percent(100);
            bgLayer.style.height = Length.Percent(100);
            SetSprite(bgLayer, GetSprite("industrial_bg"));
            Add(bgLayer);

            // Custom Painter2D overlay for safe boundary lines & hazard floor hatching
            generateVisualContent += PaintOverlay;

            // 2. Casualty Element
            casualtyElement = CreateLayer("casualty-element");
            SetSprite(casualtyElement, GetSprite("casualty_lying"));
            casualtyElement.style.width = Length.Percent(34);
            casualtyElement.style.height = Length.Percent(22);
            casualtyElement.style.left = Length.Percent(44);
            casualtyElement.style.top = Length.Percent(58);
            Add(casualtyElement);

            // 3. Bystander Element
            bystanderElement = CreateLayer("bystander-element");
            SetSprite(bystanderElement, GetSprite("bystander_anxious"));
            bystanderElement.style.width = Length.Percent(15);
            bystanderElement.style.height = Length.Percent(35);
            bystanderElement.style.left = Length.Percent(26);
            bystanderElement.style.top = Length.Percent(40);
            Add(bystanderElement);

            // 4. Player Responder Element
            playerElement = CreateLayer("player-element");
            SetSprite(playerElement, GetSprite("player_idle"));
            playerElement.style.width = Length.Percent(20);
            playerElement.style.height = Length.Percent(46);
            playerElement.style.left = Length.Percent(14);
            playerElement.style.top = Length.Percent(45);
            Add(playerElement);

            // 5. FX Elements
            arcElement = CreateLayer("arc-element");
            SetSprite(arcElement, GetSprite("electrical_arc"));
            arcElement.style.width = Length.Percent(18);
            arcElement.style.height = Length.Percent(28);
            arcElement.style.left = Length.Percent(50);
            arcElement.style.top = Length.Percent(28);
            arcElement.style.display = DisplayStyle.None;
            Add(arcElement);

            hazardIconElement = CreateLayer("hazard-element");
            SetSprite(hazardIconElement, GetSprite("hazard_warning"));
            hazardIconElement.style.width = Length.Percent(12);
            hazardIconElement.style.height = Length.Percent(16);
            hazardIconElement.style.left = Length.Percent(53);
            hazardIconElement.style.top = Length.Percent(22);
            hazardIconElement.style.display = DisplayStyle.None;
            Add(hazardIconElement);

            scanElement = CreateLayer("scan-element");
            SetSprite(scanElement, GetSprite("scan_reticle"));
            scanElement.style.width = Length.Percent(22);
            scanElement.style.height = Length.Percent(32);
            scanElement.style.left = Length.Percent(48);
            scanElement.style.top = Length.Percent(45);
            scanElement.style.display = DisplayStyle.None;
            Add(scanElement);

            callElement = CreateLayer("call-element");
            SetSprite(callElement, GetSprite("call_icon"));
            callElement.style.width = Length.Percent(8);
            callElement.style.height = Length.Percent(12);
            callElement.style.left = Length.Percent(18);
            callElement.style.top = Length.Percent(35);
            callElement.style.display = DisplayStyle.None;
            Add(callElement);

            // 6. Warning Vignette Overlay
            warningVignette = CreateLayer("warning-vignette");
            warningVignette.style.left = 0;
            warningVignette.style.top = 0;
            warningVignette.style.width = Length.Percent(100);
            warningVignette.style.height = Length.Percent(100);
            warningVignette.style.backgroundColor = new Color(0.9f, 0.2f, 0.1f, 0.18f);
            warningVignette.style.display = DisplayStyle.None;
            Add(warningVignette);

            UpdateState();
        }

        private void LoadSprites()
        {
            string[] names = {
                "industrial_bg", "player_idle", "player_approach", "player_assess",
                "player_direct", "player_recoil", "player_call", "player_reach", "player_monitor",
                "casualty_lying", "bystander_anxious", "electrical_arc", "hazard_warning",
                "scan_reticle", "call_icon"
            };

            foreach (var name in names)
            {
                string path = name == "industrial_bg" ? "Sprites/Environment/" + name
                            : name.StartsWith("player") || name.StartsWith("casualty") || name.StartsWith("bystander") ? "Sprites/Characters/" + name
                            : "Sprites/Effects/" + name;

                Sprite s = Resources.Load<Sprite>(path);
                if (s != null)
                {
                    sprites[name] = s;
                }
            }
        }

        private Sprite GetSprite(string name)
        {
            sprites.TryGetValue(name, out var s);
            return s;
        }

        private static VisualElement CreateLayer(string cls)
        {
            var element = new VisualElement();
            element.AddToClassList(cls);
            if (cls != "bg-layer" && cls != "warning-vignette")
            {
                element.AddToClassList("sprite-layer");
            }
            element.style.position = Position.Absolute;
            element.pickingMode = PickingMode.Ignore;
            return element;
        }

        private static void SetSprite(VisualElement element, Sprite sprite)
        {
            if (sprite != null)
            {
                element.style.backgroundImage = new StyleBackground(sprite);
            }
        }

        public new void MarkDirtyRepaint()
        {
            UpdateState();
            base.MarkDirtyRepaint();
        }

        private void UpdateState()
        {
            float t = ResponseProgress;
            float travel = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / 0.48f));
            float retreat = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - 0.62f) / 0.3f));
            float excursion = travel * (1 - 0.3f * retreat);
            float movement = ReducedMotion ? 0.7f : excursion;

            bool approach = Response == WorldResponse.Approach;
            bool reach = Response == WorldResponse.Reach;
            bool leaving = Response == WorldResponse.Leave;
            bool directing = Response == WorldResponse.DirectBystander;
            bool calling = Response == WorldResponse.Call || CommunicationVisible;
            bool surveying = Response == WorldResponse.Survey || Response == WorldResponse.Monitor || AssessmentVisible;

            // Player position interpolation
            float pLeft = approach ? Mathf.Lerp(14, 42, movement)
                        : reach ? Mathf.Lerp(14, 52, movement)
                        : leaving ? Mathf.Lerp(14, 2, ReducedMotion ? 1 : travel)
                        : 14;

            float pTop = approach ? Mathf.Lerp(45, 42, movement)
                       : reach ? Mathf.Lerp(45, 38, movement)
                       : leaving ? Mathf.Lerp(45, 52, ReducedMotion ? 1 : travel)
                       : 45;

            playerElement.style.left = Length.Percent(pLeft);
            playerElement.style.top = Length.Percent(pTop);

            // Player pose sprite selection
            string pose = "player_idle";
            if ((approach || reach) && t > 0.6f) pose = "player_recoil";
            else if (reach) pose = "player_reach";
            else if (approach || leaving) pose = "player_approach";
            else if (directing) pose = "player_direct";
            else if (calling) pose = "player_call";
            else if (surveying) pose = "player_assess";

            SetSprite(playerElement, GetSprite(pose));

            // Bystander movement & state
            float bLeft = directing ? Mathf.Lerp(26, 45, ReducedMotion ? 0.7f : excursion) : 26;
            float bTop = directing ? Mathf.Lerp(40, 52, ReducedMotion ? 0.7f : excursion) : 40;
            bystanderElement.style.left = Length.Percent(bLeft);
            bystanderElement.style.top = Length.Percent(bTop);

            // Electrical Arc & Hazard Warnings
            bool electricalActive = approach || reach || directing;
            bool showArc = electricalActive && (ReducedMotion || (t > 0.40f && t < 0.85f));
            arcElement.style.display = showArc ? DisplayStyle.Flex : DisplayStyle.None;
            if (showArc && !ReducedMotion)
            {
                float flickerAlpha = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 45f);
                arcElement.style.opacity = flickerAlpha;
            }

            hazardIconElement.style.display = (Warning || (electricalActive && t > 0.5f)) ? DisplayStyle.Flex : DisplayStyle.None;
            if (!ReducedMotion && hazardIconElement.style.display == DisplayStyle.Flex)
            {
                float pulseScale = 1.0f + 0.12f * Mathf.Sin(Time.unscaledTime * 8f);
                hazardIconElement.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));
                hazardIconElement.style.scale = new Scale(new Vector2(pulseScale, pulseScale));
            }

            // Scan reticle during survey / assessment
            scanElement.style.display = surveying ? DisplayStyle.Flex : DisplayStyle.None;

            // Call icon overlay near responder
            callElement.style.display = calling ? DisplayStyle.Flex : DisplayStyle.None;
            callElement.style.left = Length.Percent(pLeft + 4);
            callElement.style.top = Length.Percent(pTop - 12);

            // Warning Vignette
            warningVignette.style.display = (Warning && Reaction > 0.05f) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void PaintOverlay(MeshGenerationContext context)
        {
            if (contentRect.width < 1) return;
            var pen = context.painter2D;
            float sx = contentRect.width / 1000f;
            float sy = contentRect.height / 640f;

            // Render crisp cyan/gold safe zone boundary lines & floor markers on top of background
            pen.strokeColor = new Color(0.38f, 0.76f, 0.72f, 0.8f);
            pen.lineWidth = 3f * sx;
            pen.BeginPath();
            pen.MoveTo(new Vector2(50 * sx, 490 * sy));
            pen.LineTo(new Vector2(290 * sx, 490 * sy));
            pen.LineTo(new Vector2(370 * sx, 620 * sy));
            pen.Stroke();

            // Hazard Clearance Boundary line (amber warning line)
            pen.strokeColor = new Color(0.9f, 0.65f, 0.25f, 0.85f);
            pen.lineWidth = 3f * sx;
            pen.BeginPath();
            pen.MoveTo(new Vector2(480 * sx, 430 * sy));
            pen.LineTo(new Vector2(880 * sx, 430 * sy));
            pen.Stroke();
        }
    }
}
