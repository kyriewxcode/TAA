using UnityEngine;

namespace TAA
{
    internal sealed class TAAData
    {
        internal Matrix4x4 projPreview;
        internal Matrix4x4 viewPreview;
        internal Matrix4x4 projJitter;
        internal Vector2 offset;
        internal float blend;
        internal TAAFeature.TAAQuality quality;
    }
}