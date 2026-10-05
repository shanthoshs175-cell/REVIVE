using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Revive.Reasoning
{
    public sealed class InferenceResult
    {
        public IReadOnlyDictionary<string, bool> Facts { get; }
        public IReadOnlyList<Rule> FiredRules { get; }
        public int Passes { get; }
        internal InferenceResult(Dictionary<string, bool> facts, List<Rule> fired, int passes)
        { Facts = new ReadOnlyDictionary<string, bool>(facts); FiredRules = fired.AsReadOnly(); Passes = passes; }
    }
    /// <summary>Monotonic fixed-point inference over explicit facts. Cycles terminate; contradictions fail closed.</summary>
    public sealed class ForwardChainer
    {
        public InferenceResult Infer(IEnumerable<Fact> input, IReadOnlyList<Rule> rules, int maxPasses = 128)
        {
            if (maxPasses < 1) throw new ArgumentOutOfRangeException(nameof(maxPasses));
            var known = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var fact in input) Add(known, fact);
            var fired = new List<Rule>();
            for (int pass = 1; pass <= maxPasses; pass++)
            {
                bool changed = false;
                foreach (var rule in rules)
                {
                    if (!RuleEngine.Matches(known, rule.Conditions)) continue;
                    bool derived = false;
                    foreach (var conclusion in rule.Conclusions) derived |= Add(known, conclusion);
                    if (derived) { changed = true; fired.Add(rule); }
                }
                if (!changed) return new InferenceResult(known, fired, pass);
            }
            throw new InvalidOperationException("Inference exceeded its pass limit; no decision was applied.");
        }
        private static bool Add(Dictionary<string, bool> known, Fact fact)
        {
            if (fact == null || string.IsNullOrWhiteSpace(fact.Id)) throw new ArgumentException("Invalid fact.");
            if (known.TryGetValue(fact.Id, out bool value))
            {
                if (value != fact.Value) throw new InvalidOperationException("Contradictory fact: " + fact.Id);
                return false;
            }
            known.Add(fact.Id, fact.Value); return true;
        }
    }
}
