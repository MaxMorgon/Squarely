using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Squarely.Presentation
{
    /// <summary>
    /// The single source of cell visuals for the board, the hand and the ghost.
    /// Resolves a cell type id to a prefab and pools instances per prefab.
    /// </summary>
    public sealed class CellVisualCatalog
    {
        private readonly Func<string, CellVisual> _resolvePrefab;
        private readonly Transform _poolRoot;
        private readonly Dictionary<CellVisual, ObjectPool<CellVisual>> _pools = new Dictionary<CellVisual, ObjectPool<CellVisual>>();

        /// <param name="resolvePrefab">Returns the visual prefab for a cell type id, e.g. <see cref="Theme.ResolveCellVisual"/>.</param>
        /// <param name="poolRoot">Parent of visuals while they are in the pool.</param>
        public CellVisualCatalog(Func<string, CellVisual> resolvePrefab, Transform poolRoot)
        {
            _resolvePrefab = resolvePrefab ?? throw new ArgumentNullException(nameof(resolvePrefab));
            _poolRoot = poolRoot;
        }

        /// <summary>An active visual for the cell type, in the Idle state, parented to <paramref name="parent"/>.</summary>
        public CellVisual Get(string typeId, Transform parent)
        {
            CellVisual prefab = _resolvePrefab(typeId);

            if (prefab == null)
                throw new InvalidOperationException($"No cell visual for cell type '{typeId}'.");

            CellVisual visual = GetPool(prefab).Get();
            visual.transform.SetParent(parent, false);
            visual.SetState(CellVisualState.Idle);
            return visual;
        }

        private ObjectPool<CellVisual> GetPool(CellVisual prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool<CellVisual> existing))
                return existing;

            ObjectPool<CellVisual> pool = null;
            pool = new ObjectPool<CellVisual>(
                createFunc: () => Create(prefab, pool),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: OnRelease,
                actionOnDestroy: visual => DestroyObject(visual.gameObject));

            _pools.Add(prefab, pool);
            return pool;
        }

        private CellVisual Create(CellVisual prefab, ObjectPool<CellVisual> pool)
        {
            CellVisual visual = Object.Instantiate(prefab, _poolRoot);
            visual.SetReleaseAction(pool.Release);
            return visual;
        }

        private void OnRelease(CellVisual visual)
        {
            visual.gameObject.SetActive(false);
            visual.transform.SetParent(_poolRoot, false);
        }

        private static void DestroyObject(GameObject gameObject)
        {
            if (Application.isPlaying)
                Object.Destroy(gameObject);
            else
                Object.DestroyImmediate(gameObject);
        }
    }
}
