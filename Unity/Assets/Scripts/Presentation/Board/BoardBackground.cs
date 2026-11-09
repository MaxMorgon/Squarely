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
        public void Build(BoardSpace boardSpace, GameObject tilePrefab)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            for (int y = 0; y < boardSpace.Height; y++)
            {
                for (int x = 0; x < boardSpace.Width; x++)
                {
                    GameObject tile = Instantiate(tilePrefab, transform);
                    tile.name = $"Tile {x},{y}";
                    tile.transform.localPosition = boardSpace.CellCenter(new GridPos(x, y));
                }
            }
        }

        /// <summary>Draws the board's outline and cell grid.</summary>
        public void DrawGizmos(BoardSpace boardSpace)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            float width = boardSpace.Width * boardSpace.CellSize;
            float height = boardSpace.Height * boardSpace.CellSize;
            float left = -width * 0.5f;
            float bottom = -height * 0.5f;

            Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

            for (int x = 1; x < boardSpace.Width; x++)
            {
                float lineX = left + x * boardSpace.CellSize;
                Gizmos.DrawLine(new Vector3(lineX, bottom), new Vector3(lineX, -bottom));
            }

            for (int y = 1; y < boardSpace.Height; y++)
            {
                float lineY = bottom + y * boardSpace.CellSize;
                Gizmos.DrawLine(new Vector3(left, lineY), new Vector3(-left, lineY));
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 0f));

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
