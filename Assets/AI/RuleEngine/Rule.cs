using System;
using System.Collections.Generic;
using UnityEngine;
namespace Revive.Reasoning
{
    [Serializable]
    public sealed class Rule
    {
        [SerializeField] private string id;
        [SerializeField] private Fact[] conditions = Array.Empty<Fact>();
        [SerializeField] private Fact[] conclusions = Array.Empty<Fact>();
        [SerializeField] private string sourceId;
        [SerializeField, TextArea] private string rationale;
        public string Id => id;
        public IReadOnlyList<Fact> Conditions => conditions;
        public IReadOnlyList<Fact> Conclusions => conclusions;
        public string SourceId => sourceId;
        public string Rationale => rationale;
        public Rule(string id, Fact[] conditions, Fact[] conclusions, string sourceId = "", string rationale = "")
        { this.id = id; this.conditions = conditions; this.conclusions = conclusions; this.sourceId = sourceId; this.rationale = rationale; }
    }
    public static class RuleEngine
    {
        public static bool Matches(IReadOnlyDictionary<string, bool> known, IReadOnlyList<Fact> conditions)
        {
            foreach (var fact in conditions)
                if (!known.TryGetValue(fact.Id, out var value) || value != fact.Value) return false;
            return true;
        }
    }
}
