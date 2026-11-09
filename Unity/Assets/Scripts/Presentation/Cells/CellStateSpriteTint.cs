using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows a cell view's state by tinting a sprite.
    /// </summary>
    [RequireComponent(typeof(CellView))]
    public sealed class CellStateSpriteTint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _idle = Color.white;
        [SerializeField] private Color _ghost = new Color(1f, 1f, 1f, 0.35f);

        private CellView _view;

        private void Awake()
        {
            _view = GetComponent<CellView>();
        }

        private void OnEnable()
        {
            _view.StateChanged += Apply;
            Apply(_view.State);
        }

        private void OnDisable()
        {
            _view.StateChanged -= Apply;
        }

        private void Apply(CellViewState state)
        {
            _renderer.color = state == CellViewState.Ghost ? _ghost : _idle;
        }
    }
}
