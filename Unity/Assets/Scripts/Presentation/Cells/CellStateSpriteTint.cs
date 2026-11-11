using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows a cell view's state by tinting a sprite, multiplied by the view's <see cref="CellView.Tint"/>.
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
            _view.StateChangeEvent += OnStateChanged;
            _view.TintChangeEvent += OnTintChanged;
            ApplyColor();
        }

        private void OnDisable()
        {
            _view.StateChangeEvent -= OnStateChanged;
            _view.TintChangeEvent -= OnTintChanged;
        }

        private void OnStateChanged(CellViewState state)
        {
            ApplyColor();
        }

        private void OnTintChanged(Color tint)
        {
            ApplyColor();
        }

        private void ApplyColor()
        {
            Color stateColor = _view.State == CellViewState.Ghost ? _ghost : _idle;
            _renderer.color = stateColor * _view.Tint;
        }
    }
}
