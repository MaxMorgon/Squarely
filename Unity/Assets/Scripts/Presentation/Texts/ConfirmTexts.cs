using System;

namespace Squarely.Presentation
{
    /// <summary>The confirm dialog's texts.</summary>
    [Serializable]
    public sealed class ConfirmTexts
    {
        public string Question = "Are you sure?";
        public string Yes = "Yes";
        public string No = "No";

        /// <summary>Asked before the player progress is reset.</summary>
        public string ResetProgressQuestion = "Reset progress? The best score is deleted.";
    }
}
