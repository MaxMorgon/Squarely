using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows a cell visual's state by tinting a sprite.
    /// </summary>
    [RequireComponent(typeof(CellVisual))]
    public sealed class SpriteStateTint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _idle = Color.white;
        [SerializeField] private Color _ghost = new Color(1f, 1f, 1f, 0.35f);

        private CellVisual _visual;

        private void Awake()
        {
            _visual = GetComponent<CellVisual>();
        }

        private void OnEnable()
        {
            _visual.StateChanged += Apply;
            Apply(_visual.State);
        }

        private void OnDisable()
        {
            _visual.StateChanged -= Apply;
        }

        private void Apply(CellVisualState state)
        {
            _renderer.color = state == CellVisualState.Ghost ? _ghost : _idle;
        }
    }
}
