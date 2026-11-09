using System;
using NUnit.Framework;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation.Tests
{
    public class LayerViewTests
    {
        private static readonly GridPos Pos = new GridPos(1, 2);

        private CellView _prefab;
        private Transform _poolRoot;
        private LayerView _layer;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("Block").AddComponent<CellView>();
            _poolRoot = new GameObject("Pool").transform;
            _layer = new GameObject("Layer").AddComponent<LayerView>();

            var viewPool = new CellViewPool(typeId => _prefab, _poolRoot);
            _layer.Initialize(new BoardSpace(3, 3, 1f), viewPool);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_layer.gameObject);
            Object.DestroyImmediate(_poolRoot.gameObject);
            Object.DestroyImmediate(_prefab.gameObject);
        }

        [Test]
        public void Spawn_PlacesViewAtCellCenter_UnderLayer()
        {
            CellView view = _layer.Spawn(Pos, "block");

            Assert.That(view.transform.parent, Is.EqualTo(_layer.transform));
            Assert.That((Vector2)view.transform.localPosition, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        public void Spawn_OccupiedSlot_Throws()
        {
            _layer.Spawn(Pos, "block");

            Assert.Throws<InvalidOperationException>(() => _layer.Spawn(Pos, "block"));
        }

        [Test]
        public void Detach_ReturnsView_AndEmptiesSlot()
        {
            CellView view = _layer.Spawn(Pos, "block");

            Assert.That(_layer.Detach(Pos), Is.SameAs(view));
            Assert.That(_layer.Detach(Pos), Is.Null);
        }

        [Test]
        public void Detach_ThenSpawn_SlotTakesNewView()
        {
            _layer.Spawn(Pos, "block");
            _layer.Detach(Pos);

            Assert.DoesNotThrow(() => _layer.Spawn(Pos, "block"));
        }
    }
}
