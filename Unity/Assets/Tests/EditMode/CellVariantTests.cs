using System.Collections.Generic;
using NUnit.Framework;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation.Tests
{
    public class CellVariantTests
    {
        private static readonly PieceShape Shape = PieceShape.FromPattern("I2", "##");

        [Test]
        public void PieceVariant_SameInstanceId_GivesSameVariant()
        {
            Assert.That(PieceVariant.Of(new PieceInstance(7, Shape)), Is.EqualTo(PieceVariant.Of(new PieceInstance(7, Shape))));
        }

        [Test]
        public void PieceVariant_IsNeverNegative()
        {
            for (int id = 0; id < 1000; id++)
                Assert.That(PieceVariant.Of(new PieceInstance(id, Shape)), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void PieceVariant_ConsecutivePieces_UseEverySpriteOfSeven()
        {
            var used = new HashSet<int>();

            for (int id = 0; id < 50; id++)
                used.Add(PieceVariant.Of(new PieceInstance(id, Shape)) % 7);

            Assert.That(used.Count, Is.EqualTo(7));
        }

        [Test]
        public void Pick_WrapsVariantAroundTheList()
        {
            Sprite[] sprites = { CreateSprite(), CreateSprite(), CreateSprite() };

            Assert.That(CellVariantSprite.Pick(sprites, 4), Is.SameAs(sprites[1]));
            Assert.That(CellVariantSprite.Pick(sprites, -1), Is.SameAs(sprites[2]));
            Assert.That(CellVariantSprite.Pick(new Sprite[0], 4), Is.Null);

            foreach (Sprite sprite in sprites)
                Object.DestroyImmediate(sprite);
        }

        [Test]
        public void LayerSpawn_SetsVariant_AndPoolResetsIt()
        {
            CellView prefab = new GameObject("Block").AddComponent<CellView>();
            Transform poolRoot = new GameObject("Pool").transform;
            LayerView layer = new GameObject("Layer").AddComponent<LayerView>();
            var viewPool = new CellViewPool(typeId => prefab, poolRoot);
            layer.Initialize(new BoardSpace(3, 3, 1f), viewPool);

            CellView view = layer.Spawn(new GridPos(0, 0), "block", 5);
            Assert.That(view.Variant, Is.EqualTo(5));

            layer.Detach(new GridPos(0, 0)).Release();
            Assert.That(viewPool.Get("block", poolRoot).Variant, Is.EqualTo(0));

            Object.DestroyImmediate(layer.gameObject);
            Object.DestroyImmediate(poolRoot.gameObject);
            Object.DestroyImmediate(prefab.gameObject);
        }

        private static Sprite CreateSprite()
        {
            return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), Vector2.zero);
        }
    }
}
