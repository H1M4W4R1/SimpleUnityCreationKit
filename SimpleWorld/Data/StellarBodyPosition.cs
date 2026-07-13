using UnityEngine;

namespace Systems.SimpleWorld.Data
{
    /// <summary>
    ///     Calculated position of a sun or moon in world space.
    /// </summary>
    public struct StellarBodyPosition
    {
        public Quaternion direction;
        public float elevation;
        public float distance;

        public StellarBodyPosition(Quaternion direction, float elevation, float distance)
        {
            this.direction = direction;
            this.elevation = elevation;
            this.distance = distance;
        }
    }
}
