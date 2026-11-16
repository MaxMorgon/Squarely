using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The static board art behind the board's cells: the frame and one empty-slot tile per slot.
    /// </summary>
    public sealed class BoardBackground : MonoBehaviour
    {
        #region Fields

        [Tooltip("Optional. A sliced sprite that frames the board. Build sizes it to the board plus the padding.")]
        [SerializeField] private SpriteRenderer _frame;

        [Tooltip("Distance from the board edge to the frame edge on each side, in world units.")]
        [SerializeField] private float _framePadding = 0.27f;

        [Tooltip("Parent of the empty-slot tiles. Build replaces all its children.")]
        [SerializeField] private Transform _tileRoot;

        #endregion

        #region Properties

        private Transform TileRoot => _tileRoot != null ? _tileRoot : transform;

        #endregion

        #region Methods

        /// <summary>Sizes the frame to the board and replaces any existing tiles with one tile per board slot.</summary>
        public void Build(BoardSpace boardSpace, GameObject tilePrefab)
        {
            if (_frame != null)
                _frame.size = FrameSize(boardSpace);

            Transform tileRoot = TileRoot;

            for (int i = tileRoot.childCount - 1; i >= 0; i--)
                Destroy(tileRoot.GetChild(i).gameObject);

            for (int y = 0; y < boardSpace.Height; y++)
            {
                for (int x = 0; x < boardSpace.Width; x++)
                {
                    GameObject tile = Instantiate(tilePrefab, tileRoot);
                    tile.name = $"Tile {x},{y}";
                    tile.transform.localPosition = boardSpace.CellCenter(new GridPos(x, y));
                }
            }
        }

        /// <summary>Draws the board's outline, cell grid and frame outline.</summary>
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

            if (_frame != null)
            {
                Gizmos.color = new Color(1f, 0.6f, 1f, 0.6f);
                Gizmos.DrawWireCube(Vector3.zero, FrameSize(boardSpace));
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private Vector2 FrameSize(BoardSpace boardSpace)
        {
            float padding = 2f * _framePadding;
            return new Vector2(boardSpace.Width * boardSpace.CellSize + padding, boardSpace.Height * boardSpace.CellSize + padding);
        }

        #endregion
    }
}
