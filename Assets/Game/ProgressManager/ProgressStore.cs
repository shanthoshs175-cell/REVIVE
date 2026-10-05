using System;
using UnityEngine;
namespace Revive.Game
{
    public interface IProgressStore { int BestScore(string scenarioId); bool IsComplete(string scenarioId); void Record(string scenarioId, int score, string unlocks = ""); bool IsUnlocked(string scenarioId); }
    /// <summary>Local completion only; no analytics or external services.</summary>
    public sealed class PlayerPrefsProgressStore : IProgressStore
    {
        private const string Prefix = "revive.progress.v1.";
        public int BestScore(string id) => PlayerPrefs.GetInt(Prefix + id + ".best", 0);
        public bool IsComplete(string id) => PlayerPrefs.GetInt(Prefix + id + ".done", 0) == 1;
        public bool IsUnlocked(string id) => PlayerPrefs.GetInt(Prefix + id + ".unlocked", 0) == 1;
        public void Record(string id, int score, string unlocks = "")
        {
            PlayerPrefs.SetInt(Prefix + id + ".best", Math.Max(BestScore(id), score));
            PlayerPrefs.SetInt(Prefix + id + ".done", 1);
            if (!string.IsNullOrEmpty(unlocks)) PlayerPrefs.SetInt(Prefix + unlocks + ".unlocked", 1);
            PlayerPrefs.Save();
        }
    }
}
