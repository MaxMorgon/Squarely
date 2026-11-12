using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ConfigFingerprintTests
    {
        // Pinned on purpose: a change means every recorded replay stops matching. Change it only together with a replay format bump.
        private const ulong PinnedValue = 8639028774229987887UL;

        #region Stability

        [Test]
        public void Compute_SameConfig_GivesSameValue()
        {
            Assert.That(ConfigFingerprint.Compute(FullConfig()), Is.EqualTo(ConfigFingerprint.Compute(FullConfig())));
        }

        [Test]
        public void Compute_IsAFixedValue_OnEveryMachine()
        {
            var config = new SimConfig { Pieces = new List<PieceDefinition> { new PieceDefinition("dot", "#") } };

            Assert.That(ConfigFingerprint.Compute(config), Is.EqualTo(PinnedValue));
        }

        [Test]
        public void Sim_ExposesTheFingerprintOfItsConfig()
        {
            SimConfig config = DefaultSimConfig.Create();
            var sim = new Sim(config, 1);

            Assert.That(sim.ConfigFingerprint, Is.EqualTo(ConfigFingerprint.Compute(config)));
            Assert.That(sim.Clone().ConfigFingerprint, Is.EqualTo(sim.ConfigFingerprint));
        }

        #endregion

        #region Change detection

        [Test]
        public void Compute_ListSizeChange_ChangesValue()
        {
            SimConfig config = FullConfig();
            ulong before = ConfigFingerprint.Compute(config);

            config.Pieces.Add(new PieceDefinition("extra", "##"));

            Assert.That(ConfigFingerprint.Compute(config), Is.Not.EqualTo(before));
        }

        [Test]
        public void Compute_NeighbouringStrings_DoNotBlur()
        {
            SimConfig a = FullConfig();
            SimConfig b = FullConfig();
            a.DealerId = "ab";
            a.ScoringRuleId = "c";
            b.DealerId = "a";
            b.ScoringRuleId = "bc";

            Assert.That(ConfigFingerprint.Compute(a), Is.Not.EqualTo(ConfigFingerprint.Compute(b)));
        }

        #endregion

        #region Guard

        /// <summary>
        /// Fails when someone adds a config field and forgets <see cref="ConfigFingerprint"/>:
        /// every field in the config graph is changed on its own, and the value must change each time.
        /// </summary>
        [Test]
        public void EveryConfigField_ChangesTheFingerprint()
        {
            SimConfig config = FullConfig();
            CheckFields(config, config, "SimConfig");
        }

        private static void CheckFields(SimConfig root, object node, string path)
        {
            foreach (FieldInfo field in node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldPath = path + "." + field.Name;
                object value = field.GetValue(node);

                if (value is IList list)
                {
                    CheckList(root, list, fieldPath);
                    continue;
                }

                if (IsSimpleType(field.FieldType))
                {
                    CheckValueChanges(root, node, field, fieldPath);
                    continue;
                }

                Assert.That(field.FieldType.IsClass, Is.True, $"{fieldPath}: type {field.FieldType.Name} is not handled by this guard test.");
                CheckFields(root, value, fieldPath);
            }
        }

        private static void CheckList(SimConfig root, IList list, string path)
        {
            Assert.That(list.Count, Is.GreaterThan(0), $"{path}: the test config must fill every list.");

            if (list[0] is string original)
            {
                list[0] = original + "x";
                Assert.That(ConfigFingerprint.Compute(root), Is.Not.EqualTo(ConfigFingerprint.Compute(FullConfig())), $"{path} is not part of the fingerprint.");
                list[0] = original;
                return;
            }

            CheckFields(root, list[0], path + "[0]");
        }

        private static void CheckValueChanges(SimConfig root, object owner, FieldInfo field, string path)
        {
            object original = field.GetValue(owner);
            ulong before = ConfigFingerprint.Compute(root);

            field.SetValue(owner, ChangedValue(original));
            ulong after = ConfigFingerprint.Compute(root);
            field.SetValue(owner, original);

            Assert.That(after, Is.Not.EqualTo(before), $"{path} is not part of the fingerprint.");
        }

        private static bool IsSimpleType(Type type)
        {
            return type == typeof(int) || type == typeof(bool) || type == typeof(string);
        }

        private static object ChangedValue(object value)
        {
            if (value is int number)
                return number + 1;

            if (value is bool flag)
                return !flag;

            return ((string)value ?? string.Empty) + "x";
        }

        /// <summary>A config with every list filled, so each field can be changed on its own.</summary>
        private static SimConfig FullConfig()
        {
            SimConfig config = DefaultSimConfig.Create();
            config.Clearing.Patterns.Add(new BoardPatternDefinition
            {
                Id = "pattern",
                Pattern = "##/##",
                Points = 5,
                IsAnchored = true,
                AnchorX = 1,
                AnchorY = 2,
            });

            return config;
        }

        #endregion
    }
}
