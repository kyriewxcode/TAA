using UnityEngine;

namespace TAA
{
    public class TAAUtils
    {
        private const int k_LoopFrameCount = 9;

        private static Vector2[] HaltonSequence9 = new Vector2[]
        {
            new Vector2(0.5f, 1.0f / 3f),
            new Vector2(0.25f, 2.0f / 3f),
            new Vector2(0.75f, 1.0f / 9f),
            new Vector2(0.125f, 4.0f / 9f),
            new Vector2(0.625f, 7.0f / 9f),
            new Vector2(0.375f, 2.0f / 9f),
            new Vector2(0.875f, 5.0f / 9f),
            new Vector2(0.0625f, 8.0f / 9f),
            new Vector2(0.5625f, 1.0f / 27f),
        };

        public static Vector2 GetHaltonSequence9() => HaltonSequence9[Time.frameCount % k_LoopFrameCount];

        /// <summary>
        /// 获取Jitter后正交投影矩阵
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="jitter"></param>
        /// <returns></returns>
        public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2 jitter)
        {
            float vertical = camera.orthographicSize;
            float horizontal = vertical * camera.aspect;

            jitter.x *= horizontal / (0.5f * camera.pixelWidth);
            jitter.y *= vertical / (0.5f * camera.pixelHeight);

            float left = jitter.x - horizontal;
            float right = jitter.x + horizontal;
            float top = jitter.y + vertical;
            float bottom = jitter.y - vertical;

            return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
        }

        /// <summary>
        /// 获取Jitter后透视投影矩阵
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="jitter"></param>
        /// <returns></returns>
        public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2 jitter)
        {
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;

            float vertical = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView) * near;
            float horizontal = vertical * camera.aspect;

            jitter.x *= horizontal / (0.5f * camera.pixelWidth);
            jitter.y *= vertical / (0.5f * camera.pixelHeight);

            var matrix = camera.projectionMatrix;

            matrix[0, 2] += jitter.x / horizontal;
            matrix[1, 2] += jitter.y / vertical;

            return matrix;
        }

        public static void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
        {
            if (array == null || array.Length != size)
            {
                array = new T[size];
                for (int i = 0; i != size; i++)
                    array[i] = initialValue;
            }
        }

        public static void EnsureRenderTarget(
            ref RenderTexture rt,
            int width, int height,
            RenderTextureFormat format,
            FilterMode filterMode,
            int depthBits = 0,
            int antiAliasing = 1)
        {
            if (rt != null && (rt.width != width || rt.height != height || rt.format != format || rt.filterMode != filterMode || rt.antiAliasing != antiAliasing))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Clamp;
            }
        }
    }
}