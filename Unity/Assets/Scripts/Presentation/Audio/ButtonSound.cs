using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Add this to a Button to play the shared button-tap cue (<see cref="SoundHooks.ButtonTap"/>) when it is clicked.
    /// It stays silent until its screen's owner calls <see cref="Bind"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSound : MonoBehaviour
    {
        private SoundPlayer _sound;

        #region Unity

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        #endregion

        /// <summary>Connects every ButtonSound under the root, also inactive ones, to the sound player. Safe to call again.</summary>
        public static void Bind(GameObject root, SoundPlayer sound)
        {
            foreach (ButtonSound buttonSound in root.GetComponentsInChildren<ButtonSound>(true))
                buttonSound._sound = sound;
        }

        private void OnClicked()
        {
            _sound?.PlayHook(hooks => hooks.ButtonTap);
        }
    }
}
