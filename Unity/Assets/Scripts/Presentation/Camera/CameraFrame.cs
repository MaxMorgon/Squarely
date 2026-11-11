using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Where an orthographic camera looks and how much it shows.</summary>
    public readonly struct CameraFrame
    {
        /// <summary>The camera's position on the view plane, in world units.</summary>
        public Vector2 Center { get; }

        /// <summary>Half the view's height, in world units (<see cref="Camera.orthographicSize"/>).</summary>
        public float OrthographicSize { get; }

        public CameraFrame(Vector2 center, float orthographicSize)
        {
            Center = center;
            OrthographicSize = orthographicSize;
        }
    }
}
