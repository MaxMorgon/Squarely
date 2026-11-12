using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>A range of sound levels with an optional lowest and highest level. With no limit set, every level is in range.</summary>
    [Serializable]
    public sealed class SoundLevelRange
    {
        [SerializeField] private bool _hasMin;
        [SerializeField] private int _minLevel;
        [SerializeField] private bool _hasMax;
        [SerializeField] private int _maxLevel;

        public bool HasMin => _hasMin;

        public int MinLevel => _minLevel;

        public bool HasMax => _hasMax;

        public int MaxLevel => _maxLevel;

        public SoundLevelRange()
        {
        }

        public SoundLevelRange(int? minLevel, int? maxLevel)
        {
            _hasMin = minLevel.HasValue;
            _minLevel = minLevel ?? 0;
            _hasMax = maxLevel.HasValue;
            _maxLevel = maxLevel ?? 0;
        }

        /// <summary>Whether the level is at or above the lowest level and at or below the highest level, where set.</summary>
        public bool Contains(int level)
        {
            if (_hasMin && level < _minLevel)
                return false;

            return !_hasMax || level <= _maxLevel;
        }
    }
}
