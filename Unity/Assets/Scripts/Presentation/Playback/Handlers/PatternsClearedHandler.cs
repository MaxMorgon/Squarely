using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Detaches the cleared cells' views from the board and plays the clear cues once per match, all matches at once.
    /// A cell shared by two matches is animated by the first. The views go back to the pool when their match's cues end.
    /// </summary>
    public sealed class PatternsClearedHandler : SimEventHandler<PatternsClearedEvent>
    {
        private readonly CueSequence _cues;

        public PatternsClearedHandler(CueSequence cues)
        {
            _cues = cues;
        }

        protected override void Handle(PatternsClearedEvent simEvent, PlaybackContext context)
        {
            PlaybackViews views = context.Views;

            foreach (ClearedMatch match in simEvent.Matches)
            {
                var cells = new List<CellView>(match.Cells.Count);
                var positions = new List<GridPos>(match.Cells.Count);

                foreach (GridPos pos in match.Cells)
                {
                    CellView view = views.Board.Detach(pos);

                    if (view == null)
                        continue;

                    cells.Add(view);
                    positions.Add(pos);
                }

                // A sequence that does not wait may still be animating these views, e.g. the placement.
                context.SnapRunsOn(cells);

                Vector3 center = views.CenterOf(match.Cells);
                context.LastEffectPosition = center;
                context.Play(_cues, CueTarget.ForCells(cells, positions, center), () => Release(cells));
            }
        }

        private static void Release(List<CellView> cells)
        {
            foreach (CellView cell in cells)
                cell.Release();
        }
    }
}
