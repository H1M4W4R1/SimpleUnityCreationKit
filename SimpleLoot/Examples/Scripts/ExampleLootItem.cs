using Systems.SimpleLoot.Abstract.Interfaces;
using UnityEngine;

namespace Systems.SimpleLoot.Examples.Scripts
{
    /// <summary>
    ///     Example loot item demonstrating direct <see cref="IWithChance"/> integration.
    ///     Designers set <see cref="_chance"/> in the inspector; the weighted generator
    ///     picks it up automatically without needing a <see cref="Abstract.Rarity.RarityBase"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ExampleLootItem", menuName = "SimpleLoot/Examples/Loot Item")]
    public sealed class ExampleLootItem : ScriptableObject, IWithChance
    {
        [SerializeField] private string _displayName;
        [SerializeField] [Range(0f, 100f)] private float _chance = 10f;
        [SerializeField] private bool _isRare;
        [SerializeField] private bool _isLocked;

        public string DisplayName => _displayName;
        public float Chance => _chance;
        public bool IsRare => _isRare;
        public bool IsLocked => _isLocked;
    }
}
