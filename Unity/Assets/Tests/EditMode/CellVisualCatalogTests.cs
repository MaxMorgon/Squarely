using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation.Tests
{
    public class CellVisualCatalogTests
    {
        private CellVisual _blockPrefab;
        private CellVisual _stonePrefab;
        private Transform _poolRoot;
        private Transform _parent;
        private CellVisualCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _blockPrefab = new GameObject("Block").AddComponent<CellVisual>();
            _stonePrefab = new GameObject("Stone").AddComponent<CellVisual>();
            _poolRoot = new GameObject("Pool").transform;
            _parent = new GameObject("Parent").transform;
            _catalog = new CellVisualCatalog(Resolve, _poolRoot);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parent.gameObject);
            Object.DestroyImmediate(_poolRoot.gameObject);
            Object.DestroyImmediate(_stonePrefab.gameObject);
            Object.DestroyImmediate(_blockPrefab.gameObject);
        }

        [Test]
        public void Get_ReturnsActiveIdleVisual_UnderParent()
        {
            CellVisual visual = _catalog.Get("block", _parent);

            Assert.That(visual.gameObject.activeSelf, Is.True);
            Assert.That(visual.State, Is.EqualTo(CellVisualState.Idle));
            Assert.That(visual.transform.parent, Is.EqualTo(_parent));
        }

        [Test]
        public void Get_TwiceWithoutRelease_ReturnsDifferentInstances()
        {
            CellVisual first = _catalog.Get("block", _parent);
            CellVisual second = _catalog.Get("block", _parent);

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Release_DeactivatesVisual_AndMovesItToPoolRoot()
        {
            CellVisual visual = _catalog.Get("block", _parent);

            visual.Release();

            Assert.That(visual.gameObject.activeSelf, Is.False);
            Assert.That(visual.transform.parent, Is.EqualTo(_poolRoot));
        }

        [Test]
        public void Get_AfterRelease_ReusesInstance_InIdleState()
        {
            CellVisual first = _catalog.Get("block", _parent);
            first.SetState(CellVisualState.Ghost);
            first.Release();

            CellVisual second = _catalog.Get("block", _parent);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.State, Is.EqualTo(CellVisualState.Idle));
        }

        [Test]
        public void Get_ReleasedVisualOfOtherType_IsNotReused()
        {
            CellVisual block = _catalog.Get("block", _parent);
            block.Release();

            CellVisual stone = _catalog.Get("stone", _parent);

            Assert.That(stone, Is.Not.SameAs(block));
            Assert.That(stone.name, Does.StartWith("Stone"));
        }

        [Test]
        public void Get_UnresolvedType_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _catalog.Get("unknown", _parent));
        }

        [Test]
        public void Release_VisualNotFromCatalog_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _blockPrefab.Release());
        }

        private CellVisual Resolve(string typeId)
        {
            switch (typeId)
            {
                case "block":
                    return _blockPrefab;

                case "stone":
                    return _stonePrefab;

                default:
                    return null;
            }
        }
    }
}
