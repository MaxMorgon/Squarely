#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Editor and dev-tools builds only (`SQUARELY_DEV_TOOLS`): a ring that follows the pointer, drawn over everything.
    /// It makes the pointer visible in screen recordings. The ring shrinks and its inside fills while the pointer is pressed.
    /// Does not block input. Hidden until <see cref="Show"/> runs.
    /// </summary>
    public sealed class CursorMarker : MonoBehaviour
    {
        #region Constants and fields

        private const int SortingOrder = short.MaxValue;
        private const int TextureSize = 128;

        /// <summary>The ring width as a fraction of the ring radius.</summary>
        private const float RingWidth = 0.14f;

        /// <summary>The ring diameter as a fraction of the screen height.</summary>
        private const float SizeOfScreenHeight = 0.045f;

        private const float PressedScale = 0.8f;
        private const float FillAlpha = 0.15f;
        private const float PressedFillAlpha = 0.5f;

        private static readonly Color Color = new Color(1f, 0.85f, 0.2f, 1f);

        private RectTransform _marker;
        private Image _fill;

        #endregion

        #region Unity

        private void LateUpdate()
        {
            Pointer pointer = Pointer.current;
            bool hasPointer = pointer != null;
            _marker.gameObject.SetActive(hasPointer);
            if (!hasPointer)
                return;

            bool isPressed = pointer.press.isPressed;
            float size = Screen.height * SizeOfScreenHeight * (isPressed ? PressedScale : 1f);
            _marker.sizeDelta = new Vector2(size, size);
            _marker.position = pointer.position.ReadValue();

            Color fillColor = Color;
            fillColor.a = isPressed ? PressedFillAlpha : FillAlpha;
            _fill.color = fillColor;
        }

        #endregion

        #region Methods

        /// <summary>Creates a hidden marker on its own overlay canvas, under the parent.</summary>
        public static CursorMarker Create(Transform parent)
        {
            var root = new GameObject("CursorMarker");
            root.transform.SetParent(parent, false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var marker = root.AddComponent<CursorMarker>();
            marker._marker = CreateRect("Marker", root.transform);
            marker._fill = CreateImage("Fill", marker._marker, CreateSprite(0f));
            CreateImage("Ring", marker._marker, CreateSprite(1f - RingWidth)).color = Color;

            marker.Hide();
            return marker;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>A white anti-aliased disc, or a ring when <paramref name="innerRadius"/> (a fraction of the radius) is above 0.</summary>
        private static Sprite CreateSprite(float innerRadius)
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float radius = TextureSize * 0.5f;
            float edge = 1f / radius;
            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float dx = (x + 0.5f - radius) / radius;
                    float dy = (y + 0.5f - radius) / radius;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float outer = Mathf.Clamp01((1f - distance) / edge);
                    float inner = innerRadius > 0f ? Mathf.Clamp01((distance - innerRadius) / edge) : 1f;
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, (byte)(255 * outer * inner));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f));
        }

        #endregion
    }
}
#endif
