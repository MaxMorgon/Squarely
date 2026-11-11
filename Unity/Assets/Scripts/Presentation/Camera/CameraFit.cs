using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Fits an orthographic camera's view to an area of the world.</summary>
    public static class CameraFit
    {
        /// <summary>
        /// The smallest frame that shows the whole area plus a margin on every side, below a band at the top of the
        /// screen that the area must stay out of. The area is centered in the rest of the screen.
        /// </summary>
        /// <param name="area">The world area to show.</param>
        /// <param name="aspect">The screen's width divided by its height.</param>
        /// <param name="margin">Free space around the area, in world units.</param>
        /// <param name="topBand">The share of the screen's height at the top to keep the area out of, from 0 (none) up to but excluding 1.</param>
        public static CameraFrame Fit(Rect area, float aspect, float margin, float topBand)
        {
            if (aspect <= 0f)
                throw new ArgumentOutOfRangeException(nameof(aspect), aspect, "Aspect must be positive.");

            if (topBand < 0f || topBand >= 1f)
                throw new ArgumentOutOfRangeException(nameof(topBand), topBand, "Top band must be in [0, 1).");

            float width = area.width + 2f * margin;
            float height = area.height + 2f * margin;
            float freeShare = 1f - topBand;

            float viewHeight = Mathf.Max(height / freeShare, width / aspect);
            float orthographicSize = viewHeight * 0.5f;

            // The free part of the screen is centered topBand * orthographicSize below the screen's center.
            var center = new Vector2(area.center.x, area.center.y + topBand * orthographicSize);
            return new CameraFrame(center, orthographicSize);
        }
    }
}
