using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Keeps the game camera framed on the gameplay area (board and hand) plus a margin, below the HUD's top band,
    /// for any screen size or aspect. Refits when the screen size, the band or the margin changes.
    /// Does nothing until its owner calls <see cref="Initialize"/>.
    /// </summary>
    public sealed class CameraFitter : MonoBehaviour
    {
        // The band never takes more than this share of the screen, so a misconfigured band can't push the area off-screen.
        private const float MaxTopBandShare = 0.9f;

        [Tooltip("Free space around the board and hand, in world units.")]
        [SerializeField] private float _margin = 0.4f;

        [Tooltip("The strip at the top of the HUD that the board and hand stay below. Anchored to the canvas's top edge; only its height counts. None keeps the whole screen free.")]
        [SerializeField] private RectTransform _topBand;

        private Camera _camera;
        private Rect _area;
        private Canvas _topBandCanvas;

        private bool _isFitted;
        private int _fittedPixelWidth;
        private int _fittedPixelHeight;
        private float _fittedTopBand;
        private float _fittedMargin;

        /// <summary>Frames the camera on a world area and keeps it framed. Call once.</summary>
        public void Initialize(Camera camera, Rect area)
        {
            _camera = camera;
            _area = area;

            if (_topBand != null)
                _topBandCanvas = _topBand.GetComponentInParent<Canvas>().rootCanvas;

            FitIfChanged();
        }

        #region Unity

        // LateUpdate, so the canvas scalers have already updated their scale for this frame's screen size.
        private void LateUpdate()
        {
            if (_camera == null)
                return;

            FitIfChanged();
        }

        #endregion

        /// <summary>Draws the area the camera keeps on screen: the gameplay area plus the margin.</summary>
        public void DrawGizmos(Rect area)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(area.center, area.size + Vector2.one * (2f * _margin));
        }

        private void FitIfChanged()
        {
            int pixelWidth = _camera.pixelWidth;
            int pixelHeight = _camera.pixelHeight;
            float topBand = TopBandShare(pixelHeight);

            bool isUnchanged = _isFitted
                && pixelWidth == _fittedPixelWidth
                && pixelHeight == _fittedPixelHeight
                && topBand == _fittedTopBand
                && _margin == _fittedMargin;

            if (isUnchanged || pixelWidth <= 0 || pixelHeight <= 0)
                return;

            CameraFrame frame = CameraFit.Fit(_area, (float)pixelWidth / pixelHeight, _margin, topBand);
            _camera.orthographicSize = frame.OrthographicSize;
            _camera.transform.position = new Vector3(frame.Center.x, frame.Center.y, _camera.transform.position.z);

            _isFitted = true;
            _fittedPixelWidth = pixelWidth;
            _fittedPixelHeight = pixelHeight;
            _fittedTopBand = topBand;
            _fittedMargin = _margin;
        }

        private float TopBandShare(int pixelHeight)
        {
            if (_topBand == null || pixelHeight <= 0)
                return 0f;

            float bandPixels = _topBand.rect.height * _topBandCanvas.scaleFactor;
            return Mathf.Clamp(bandPixels / pixelHeight, 0f, MaxTopBandShare);
        }
    }
}
