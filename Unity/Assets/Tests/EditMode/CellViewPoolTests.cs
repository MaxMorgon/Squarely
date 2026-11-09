using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation.Tests
{
    public class CellViewPoolTests
    {
        private CellView _blockPrefab;
        private CellView _stonePrefab;
        private Transform _poolRoot;
        private Transform _parent;
        private CellViewPool _viewPool;

        [SetUp]
        public void SetUp()
        {
            _blockPrefab = new GameObject("Block").AddComponent<CellView>();
            _stonePrefab = new GameObject("Stone").AddComponent<CellView>();
            _poolRoot = new GameObject("Pool").transform;
            _parent = new GameObject("Parent").transform;
            _viewPool = new CellViewPool(Resolve, _poolRoot);
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
        public void Get_ReturnsActiveIdleView_UnderParent()
        {
            CellView view = _viewPool.Get("block", _parent);

            Assert.That(view.gameObject.activeSelf, Is.True);
            Assert.That(view.State, Is.EqualTo(CellViewState.Idle));
            Assert.That(view.transform.parent, Is.EqualTo(_parent));
        }

        [Test]
        public void Get_TwiceWithoutRelease_ReturnsDifferentInstances()
        {
            CellView first = _viewPool.Get("block", _parent);
            CellView second = _viewPool.Get("block", _parent);

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Release_DeactivatesView_AndMovesItToPoolRoot()
        {
            CellView view = _viewPool.Get("block", _parent);

            view.Release();

            Assert.That(view.gameObject.activeSelf, Is.False);
            Assert.That(view.transform.parent, Is.EqualTo(_poolRoot));
        }

        [Test]
        public void Get_AfterRelease_ReusesInstance_InIdleState()
        {
            CellView first = _viewPool.Get("block", _parent);
            first.SetState(CellViewState.Ghost);
            first.Release();

            CellView second = _viewPool.Get("block", _parent);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.State, Is.EqualTo(CellViewState.Idle));
        }

        [Test]
        public void Get_ReleasedViewOfOtherType_IsNotReused()
        {
            CellView block = _viewPool.Get("block", _parent);
            block.Release();

            CellView stone = _viewPool.Get("stone", _parent);

            Assert.That(stone, Is.Not.SameAs(block));
            Assert.That(stone.name, Does.StartWith("Stone"));
        }

        [Test]
        public void Get_UnresolvedType_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _viewPool.Get("unknown", _parent));
        }

        [Test]
        public void Release_ViewNotFromPool_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _blockPrefab.Release());
        }

        private CellView Resolve(string typeId)
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
