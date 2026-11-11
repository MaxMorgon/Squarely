using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class SimConfigTests
    {
        #region Validate

        [Test]
        public void Validate_DefaultConfig_Passes()
        {
            Assert.DoesNotThrow(() => DefaultSimConfig.Create().Validate());
        }

        [Test]
        public void Validate_ZeroWeightPiece_PassesIfAnotherHasWeight()
        {
            SimConfig config = DefaultSimConfig.Create();
            config.Pieces[0].Weight = 0;

            Assert.DoesNotThrow(() => config.Validate());
        }

        [TestCaseSource(nameof(InvalidConfigs))]
        public void Validate_InvalidConfig_Throws(Action<SimConfig> breakConfig)
        {
            SimConfig config = DefaultSimConfig.Create();
            breakConfig(config);

            Assert.Throws<InvalidOperationException>(() => config.Validate());
        }

        private static IEnumerable<TestCaseData> InvalidConfigs()
        {
            yield return Case("ZeroWidth", config => config.BoardWidth = 0);
            yield return Case("NegativeHeight", config => config.BoardHeight = -1);
            yield return Case("ZeroHandSize", config => config.HandSize = 0);
            yield return Case("NullPieces", config => config.Pieces = null);
            yield return Case("NoPieces", config => config.Pieces.Clear());
            yield return Case("NullDealing", config => config.Dealing = null);
            yield return Case("NullClearing", config => config.Clearing = null);
            yield return Case("NullScoring", config => config.Scoring = null);
            yield return Case("NullResolutionRules", config => config.ResolutionRuleIds = null);
            yield return Case("NegativeWeight", config => config.Pieces[0].Weight = -1);
            yield return Case("AllZeroWeights", config => config.Pieces.ForEach(piece => piece.Weight = 0));
        }

        private static TestCaseData Case(string name, Action<SimConfig> breakConfig)
        {
            return new TestCaseData(breakConfig).SetName($"Validate_InvalidConfig_Throws({name})");
        }

        #endregion

        #region Clone

        [Test]
        public void Clone_EqualsOriginal_AndSharesNoMutableObject()
        {
            SimConfig config = DefaultSimConfig.Create();
            config.Pieces[0].CellTypeId = "stone";
            config.Clearing.Patterns.Add(new BoardPatternDefinition
            {
                Id = "corner",
                Pattern = "##/#.",
                Points = 20,
                IsAnchored = true,
                AnchorX = 1,
                AnchorY = 2,
            });

            SimConfig copy = config.Clone();

            AssertDeepCopy(config, copy, nameof(SimConfig));
        }

        /// <summary>
        /// Walks every field: values and strings must be equal, every other object must be a different instance with equal contents.
        /// </summary>
        private static void AssertDeepCopy(object original, object copy, string path)
        {
            if (original == null)
            {
                Assert.That(copy, Is.Null, path);
                return;
            }

            Type type = original.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                Assert.That(copy, Is.EqualTo(original), path);
                return;
            }

            Assert.That(ReferenceEquals(original, copy), Is.False, $"{path} is shared, not copied.");

            if (original is IList originalList)
            {
                var copyList = (IList)copy;
                Assert.That(copyList.Count, Is.EqualTo(originalList.Count), $"{path}.Count");

                for (int i = 0; i < originalList.Count; i++)
                    AssertDeepCopy(originalList[i], copyList[i], $"{path}[{i}]");

                return;
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                AssertDeepCopy(field.GetValue(original), field.GetValue(copy), $"{path}.{field.Name}");
        }

        #endregion
    }
}
