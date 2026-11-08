using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The static empty-slot tiles behind the board's cells.
    /// </summary>
    public sealed class BoardBackground : MonoBehaviour
    {
        /// <summary>Replaces any existing tiles with one tile per board slot.</summary>
        public void Build(BoardLayout layout, GameObject tilePrefab)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    GameObject tile = Instantiate(tilePrefab, transform);
                    tile.name = $"Tile {x},{y}";
                    tile.transform.localPosition = layout.CellCenter(new GridPos(x, y));
                }
            }
        }
    }
}
