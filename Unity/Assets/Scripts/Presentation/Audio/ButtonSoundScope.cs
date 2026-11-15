using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Sits on the root of a screen and gives the <see cref="ButtonSound"/> buttons below it their sound player.
    /// A button looks it up when clicked, so buttons made after the screen opened work too.
    /// </summary>
    public sealed class ButtonSoundScope : MonoBehaviour
    {
        /// <summary>Plays the button sounds of this scope.</summary>
        public SoundPlayer Sound { get; private set; }

        /// <summary>Makes the root a scope for the sound player. Does nothing for a null player. Safe to call again.</summary>
        public static void Attach(GameObject root, SoundPlayer sound)
        {
            if (sound == null)
                return;

            ButtonSoundScope scope = root.GetComponent<ButtonSoundScope>();
            if (scope == null)
                scope = root.AddComponent<ButtonSoundScope>();

            scope.Sound = sound;
        }
    }
}
