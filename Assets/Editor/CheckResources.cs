using UnityEditor;
using UnityEngine;

namespace Revive.Editor
{
    public static class CheckResources
    {
        [MenuItem("REVIVE/Check Resource Sprites")]
        public static void Check()
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
                Texture2D t = Resources.Load<Texture2D>(path);
                Debug.Log($"REVIVE Check: '{path}' -> Sprite: {(s != null ? s.name : "NULL")}, Texture2D: {(t != null ? t.name : "NULL")}");
            }
        }
    }
}
