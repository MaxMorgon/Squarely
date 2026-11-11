using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Squarely.Presentation
{
    /// <summary>
    /// The single source of cell views for the board, the hand and the ghost.
    /// Resolves a cell type id to a prefab and pools instances per prefab.
    /// </summary>
    public sealed class CellViewPool
    {
        private readonly Func<string, CellView> _resolvePrefab;
        private readonly Transform _poolRoot;
        private readonly Dictionary<CellView, ObjectPool<CellView>> _pools = new Dictionary<CellView, ObjectPool<CellView>>();

        /// <param name="resolvePrefab">Returns the view prefab for a cell type id, e.g. <see cref="CellTheme.ResolveCellView"/>.</param>
        /// <param name="poolRoot">Parent of views while they are in the pool.</param>
        public CellViewPool(Func<string, CellView> resolvePrefab, Transform poolRoot)
        {
            _resolvePrefab = resolvePrefab ?? throw new ArgumentNullException(nameof(resolvePrefab));
            _poolRoot = poolRoot;
        }

        /// <summary>An active view for the cell type, in the Idle state with no tint and unit scale, parented to <paramref name="parent"/>.</summary>
        public CellView Get(string typeId, Transform parent)
        {
            CellView prefab = _resolvePrefab(typeId);

            if (prefab == null)
                throw new InvalidOperationException($"No cell view for cell type '{typeId}'.");

            CellView view = GetPool(prefab).Get();
            view.transform.SetParent(parent, false);
            view.transform.localScale = Vector3.one;
            view.SetTypeId(typeId);
            view.SetState(CellViewState.Idle);
            view.SetTint(Color.white);
            return view;
        }

        private ObjectPool<CellView> GetPool(CellView prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool<CellView> existing))
                return existing;

            ObjectPool<CellView> pool = null;
            pool = new ObjectPool<CellView>(
                createFunc: () => Create(prefab, pool),
                actionOnGet: view => view.gameObject.SetActive(true),
                actionOnRelease: OnRelease,
                actionOnDestroy: DestroyView);

            _pools.Add(prefab, pool);
            return pool;
        }

        private CellView Create(CellView prefab, ObjectPool<CellView> pool)
        {
            CellView view = Object.Instantiate(prefab, _poolRoot);
            view.SetReleaseAction(pool.Release);
            return view;
        }

        private void OnRelease(CellView view)
        {
            view.gameObject.SetActive(false);
            view.transform.SetParent(_poolRoot, false);
        }

        private static void DestroyView(CellView view)
        {
            // Unity clears every pool when play mode exits, after the scene's objects are already destroyed.
            if (view == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(view.gameObject);
            else
                Object.DestroyImmediate(view.gameObject);
        }
    }
}
