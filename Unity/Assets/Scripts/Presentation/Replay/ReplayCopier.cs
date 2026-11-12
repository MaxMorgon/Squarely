using System;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Copies the code of a replay to the clipboard.</summary>
    public sealed class ReplayCopier
    {
        private readonly IClipboard _clipboard;

        public ReplayCopier(IClipboard clipboard)
        {
            _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
        }

        /// <summary>Puts the code of the replay on the clipboard and returns it.</summary>
        public string Copy(GameReplay replay)
        {
            string code = ReplayCodec.Encode(replay);
            CopyCode(code);
            return code;
        }

        /// <summary>Puts a replay code on the clipboard.</summary>
        public void CopyCode(string code)
        {
            _clipboard.SetText(code);
        }
    }
}
