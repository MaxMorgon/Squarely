using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows a cell view's <see cref="CellView.Variant"/> as one sprite from a list.
    /// </summary>
    [RequireComponent(typeof(CellView))]
    public sealed class CellVariantSprite : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("One sprite per variant. The variant picks one with a modulo.")]
        [SerializeField] private Sprite[] _sprites = new Sprite[0];

        private CellView _view;

        #endregion

        #region Methods

        /// <summary>The sprite for a variant, or null if the list is empty.</summary>
        internal static Sprite Pick(Sprite[] sprites, int variant)
        {
            if (sprites == null || sprites.Length == 0)
                return null;

            int index = variant % sprites.Length;
            return sprites[index < 0 ? index + sprites.Length : index];
        }

        private void Awake()
        {
            _view = GetComponent<CellView>();
        }

        private void OnEnable()
        {
            _view.VariantChangeEvent += OnVariantChanged;
            ApplySprite();
        }

        private void OnDisable()
        {
            _view.VariantChangeEvent -= OnVariantChanged;
        }

        private void OnVariantChanged(int variant)
        {
            ApplySprite();
        }

        private void ApplySprite()
        {
            Sprite sprite = Pick(_sprites, _view.Variant);

            if (sprite != null)
                _renderer.sprite = sprite;
        }

        #endregion
    }
}
