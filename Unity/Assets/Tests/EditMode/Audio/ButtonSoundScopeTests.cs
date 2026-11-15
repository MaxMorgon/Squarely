using NUnit.Framework;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class ButtonSoundScopeTests
    {
        private GameObject _root;
        private GameObject _soundObject;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Screen");
            _soundObject = new GameObject("Sound");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_soundObject);
        }

        [Test]
        public void Attach_AddsScopeWithSound()
        {
            var sound = _soundObject.AddComponent<SoundPlayer>();

            ButtonSoundScope.Attach(_root, sound);

            Assert.AreSame(sound, _root.GetComponent<ButtonSoundScope>().Sound);
        }

        [Test]
        public void Attach_Twice_KeepsOneScopeAndUsesNewSound()
        {
            var first = _soundObject.AddComponent<SoundPlayer>();
            var second = new GameObject("Sound 2").AddComponent<SoundPlayer>();

            ButtonSoundScope.Attach(_root, first);
            ButtonSoundScope.Attach(_root, second);

            Assert.AreEqual(1, _root.GetComponents<ButtonSoundScope>().Length);
            Assert.AreSame(second, _root.GetComponent<ButtonSoundScope>().Sound);
            Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void Attach_NullSound_AddsNothing()
        {
            ButtonSoundScope.Attach(_root, null);

            Assert.IsNull(_root.GetComponent<ButtonSoundScope>());
        }

        [Test]
        public void ChildCreatedLater_FindsScope()
        {
            var sound = _soundObject.AddComponent<SoundPlayer>();
            ButtonSoundScope.Attach(_root, sound);

            var child = new GameObject("Late button");
            child.transform.SetParent(_root.transform);

            Assert.AreSame(sound, child.GetComponentInParent<ButtonSoundScope>().Sound);
        }
    }
}
