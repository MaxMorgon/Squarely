using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The hand's slots in a row, centered on this transform, each showing its piece at a reduced scale.
    /// </summary>
    public sealed class HandView : MonoBehaviour
    {
        [Tooltip("Distance between slot centers, in world units. Each slot is a square of this size for picking up pieces.")]
        [SerializeField] private float _slotSpacing = 3.3f;

        [Tooltip("Scale of hand pieces relative to the board.")]
        [SerializeField] private float _pieceScale = 0.55f;

        private CellViewPool _viewPool;
        private float _cellSize;
        private PieceView[] _slots;
        private PieceInstance[] _pieces;

        public void Initialize(int handSize, CellViewPool viewPool, float cellSize)
        {
            _viewPool = viewPool;
            _cellSize = cellSize;
            _slots = new PieceView[handSize];
            _pieces = new PieceInstance[handSize];

            for (int slot = 0; slot < handSize; slot++)
            {
                var slotObject = new GameObject($"Slot {slot}");
                slotObject.transform.SetParent(transform, false);
                slotObject.transform.localPosition = new Vector3(SlotCenterX(slot, _slots.Length), 0f, 0f);
                slotObject.transform.localScale = Vector3.one * _pieceScale;
                _slots[slot] = slotObject.AddComponent<PieceView>();
            }
        }

        /// <summary>Shows every slot of the hand and makes all slots visible.</summary>
        public void SetHand(IReadOnlyHand hand)
        {
            for (int slot = 0; slot < _slots.Length; slot++)
            {
                SetPiece(slot, hand.GetSlot(slot));
                SetSlotVisible(slot, true);
            }
        }

        /// <summary>Shows a piece in a slot, or empties the slot if the piece is null. The slot's visibility stays as it is.</summary>
        public void SetPiece(int slot, PieceInstance piece)
        {
            _pieces[slot] = piece;

            if (piece == null)
                _slots[slot].Clear();
            else
                _slots[slot].SetPiece(piece, _viewPool, _cellSize, CellViewState.Idle);
        }

        /// <summary>The view that shows a slot's piece.</summary>
        public PieceView GetSlotView(int slot)
        {
            return _slots[slot];
        }

        /// <summary>The piece shown in a slot, or null if the slot is empty.</summary>
        public PieceInstance GetPiece(int slot)
        {
            return _pieces[slot];
        }

        /// <summary>The slot whose area contains a world position, or -1 if none does.</summary>
        public int SlotAt(Vector3 worldPosition)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            float halfSize = _slotSpacing * 0.5f;

            if (Mathf.Abs(local.y) > halfSize)
                return -1;

            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (Mathf.Abs(local.x - SlotCenterX(slot, _slots.Length)) <= halfSize)
                    return slot;
            }

            return -1;
        }

        /// <summary>The area the slots' pick-up squares cover in this transform's local space, for a hand of the given size.</summary>
        public Rect LocalBounds(int slotCount)
        {
            float width = slotCount * _slotSpacing;
            return new Rect(-width * 0.5f, -_slotSpacing * 0.5f, width, _slotSpacing);
        }

        public void SetSlotVisible(int slot, bool visible)
        {
            _slots[slot].gameObject.SetActive(visible);
        }

        /// <summary>Draws each slot's pick-up area and the size of one hand cell.</summary>
        public void DrawGizmos(int handSize, float cellSize)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 slotSize = new Vector3(_slotSpacing, _slotSpacing, 0f);
            Vector3 cellGizmoSize = new Vector3(cellSize, cellSize, 0f) * _pieceScale;

            for (int slot = 0; slot < handSize; slot++)
            {
                var center = new Vector3(SlotCenterX(slot, handSize), 0f, 0f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(center, slotSize);

                Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
                Gizmos.DrawWireCube(center, cellGizmoSize);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private float SlotCenterX(int slot, int slotCount)
        {
            return (slot - (slotCount - 1) * 0.5f) * _slotSpacing;
        }
    }
}
