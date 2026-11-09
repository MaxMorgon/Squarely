using Squarely.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Squarely.Presentation
{
    /// <summary>
    /// Drag-and-drop placement: picks a piece up from the hand, shows a ghost where it would land, and places it on release.
    /// </summary>
    public sealed class PieceDragController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        [Tooltip("How far above the pointer the dragged piece floats, in world units.")]
        [SerializeField] private float _liftOffset = 2f;

        [Tooltip("Sorting order of the dragged piece, above everything on the board.")]
        [SerializeField] private int _draggedSortingOrder = 100;

        private GameController _game;
        private HandView _hand;
        private Transform _boardRoot;
        private BoardSpace _boardSpace;
        private CellViewPool _viewPool;

        private PieceView _dragged;
        private PieceView _ghost;

        private int _dragSlot = -1;
        private PieceInstance _dragPiece;
        private Vector3 _dragTarget;
        private bool _hasCheckedOrigin;
        private GridPos _checkedOrigin;
        private bool _canPlace;

        private bool IsDragging => _dragPiece != null;

        public void Initialize(GameController game, HandView hand, Transform boardRoot, BoardSpace boardSpace, CellViewPool viewPool)
        {
            _game = game;
            _hand = hand;
            _boardRoot = boardRoot;
            _boardSpace = boardSpace;
            _viewPool = viewPool;

            _dragged = CreatePieceView("Dragged Piece", transform);
            _dragged.gameObject.AddComponent<SortingGroup>().sortingOrder = _draggedSortingOrder;

            _ghost = CreatePieceView("Ghost", boardRoot);
        }

        #region Unity

        private void Update()
        {
            Pointer pointer = Pointer.current;

            if (pointer == null || _game == null)
                return;

            Vector3 world = ScreenToWorld(pointer.position.ReadValue());

            if (!IsDragging)
            {
                if (pointer.press.wasPressedThisFrame)
                    TryBeginDrag(world);

                return;
            }

            UpdateDrag(world);

            if (pointer.press.wasReleasedThisFrame)
                EndDrag();
        }

        #endregion

        #region Dragging

        private void TryBeginDrag(Vector3 world)
        {
            if (_game.IsResolved)
                return;

            int slot = _hand.SlotAt(world);

            if (slot < 0)
                return;

            PieceInstance piece = _hand.GetPiece(slot);

            if (piece == null)
                return;

            _dragSlot = slot;
            _dragPiece = piece;
            _hasCheckedOrigin = false;
            _hand.SetSlotVisible(slot, false);

            _dragged.SetShape(piece.Shape, _viewPool, _boardSpace.CellSize, CellViewState.Idle);
            _dragged.gameObject.SetActive(true);

            _ghost.SetShape(piece.Shape, _viewPool, _boardSpace.CellSize, CellViewState.Ghost);
            _ghost.gameObject.SetActive(false);

            UpdateDrag(world);
        }

        private void UpdateDrag(Vector3 world)
        {
            _dragTarget = world + Vector3.up * _liftOffset;
            MoveDraggedView();

            // Placement uses the target, not the dragged view, so view effects never shift where the piece lands.
            Vector3 originWorld = _dragTarget + _dragged.transform.parent.TransformVector(_dragged.OriginLocal);
            Vector3 originOnBoard = _boardRoot.InverseTransformPoint(originWorld);
            GridPos origin = _boardSpace.NearestCell(originOnBoard);

            if (_hasCheckedOrigin && origin == _checkedOrigin)
                return;

            _hasCheckedOrigin = true;
            _checkedOrigin = origin;
            _canPlace = _game.CanPlace(_dragPiece.InstanceId, origin);

            _ghost.gameObject.SetActive(_canPlace);

            if (_canPlace)
                _ghost.transform.localPosition = _boardSpace.CellCenter(origin) - _ghost.OriginLocal;
        }

        /// <summary>Moves the dragged piece's view toward the drag target. The single place for drag feel such as lag and tilt.</summary>
        private void MoveDraggedView()
        {
            _dragged.transform.position = _dragTarget;
        }

        private void EndDrag()
        {
            int pieceInstanceId = _dragPiece.InstanceId;
            bool place = _canPlace;
            GridPos origin = _checkedOrigin;

            _dragged.Clear();
            _dragged.gameObject.SetActive(false);
            _ghost.Clear();
            _ghost.gameObject.SetActive(false);
            _hand.SetSlotVisible(_dragSlot, true);

            _dragSlot = -1;
            _dragPiece = null;

            if (place)
                _game.Place(pieceInstanceId, origin);
        }

        #endregion

        private Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            float distanceToBoardPlane = _boardRoot.position.z - _camera.transform.position.z;
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceToBoardPlane));
            world.z = _boardRoot.position.z;
            return world;
        }

        private static PieceView CreatePieceView(string name, Transform parent)
        {
            var viewObject = new GameObject(name);
            viewObject.transform.SetParent(parent, false);
            viewObject.SetActive(false);
            return viewObject.AddComponent<PieceView>();
        }
    }
}
