using System;
using UnityEngine.Serialization;

namespace Squarely.Presentation
{
    /// <summary>The confirm dialog's texts.</summary>
    [Serializable]
    public sealed class ConfirmTexts
    {
        [FormerlySerializedAs("Question")]
        public string Title = "Are you sure?";
        public string Yes = "Yes";
        public string No = "No";

        /// <summary>Shown under the title before Restart or Quit ends a game.</summary>
        public string LeaveGameMessage = "Your score will not be saved.";

        /// <summary>Shown under the title before the player progress is reset.</summary>
        [FormerlySerializedAs("ResetProgressQuestion")]
        public string ResetProgressMessage = "Your best score will be deleted.";
    }
}
