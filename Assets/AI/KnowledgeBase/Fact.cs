using System;
using UnityEngine;
namespace Revive.Reasoning
{
    /// <summary>An explicit known boolean proposition. Missing is unknown, never false.</summary>
    [Serializable]
    public sealed class Fact : IEquatable<Fact>
    {
        [SerializeField] private string id;
        [SerializeField] private bool value = true;
        public string Id => id;
        public bool Value => value;
        public Fact(string id, bool value = true)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Fact ID is required.", nameof(id));
            this.id = id; this.value = value;
        }
        public bool Equals(Fact other) => other != null && Id == other.Id && Value == other.Value;
        public override bool Equals(object obj) => Equals(obj as Fact);
        public override int GetHashCode() => (Id?.GetHashCode() ?? 0) ^ Value.GetHashCode();
        public override string ToString() => Id + " = " + Value;
    }
}
