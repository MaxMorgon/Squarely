using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The clipboard through GUIUtility.systemCopyBuffer. Works in the editor and on desktop. Not made for mobile.</summary>
    public sealed class SystemClipboard : IClipboard
    {
        public void SetText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }
    }
}
