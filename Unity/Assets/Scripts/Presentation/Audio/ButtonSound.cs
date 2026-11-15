using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Add this to a Button to play the shared button-tap cue (<see cref="SoundHooks.ButtonTap"/>) when it is clicked.
    /// It finds its sound player in the <see cref="ButtonSoundScope"/> above it, and stays silent without one.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSound : MonoBehaviour
    {
        #region Unity

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        #endregion

        private void OnClicked()
        {
            ButtonSoundScope scope = GetComponentInParent<ButtonSoundScope>();
            if (scope != null)
                scope.Sound.PlayHook(hooks => hooks.ButtonTap);
        }
    }
}
