using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TAA
{
    public class TAAFeature : ScriptableRendererFeature
    {
        [Serializable]
        public enum TAAQuality
        {
            High,
            Medium,
            Low
        }

        private static ScriptableRendererFeature s_Instance;
        private bool isFirstFrame;

        public TAAQuality quality = TAAQuality.Medium;
        [Range(0.0f, 3.0f)] public float jitterIntensity = 1.0f;
        [Range(0.0f, 1.0f)] public float blend = 0.1f;

        private TAACameraSetupPass m_CameraSetupPass;
        private TAAPass m_TaaPass;
        Dictionary<Camera, TAAData> m_TaaDataCaches;

        Matrix4x4 viewPreview;
        Matrix4x4 projPreview;

        public override void Create()
        {
            s_Instance = this;
            isFirstFrame = true;
            name = "TAA";
            m_CameraSetupPass = new TAACameraSetupPass();
            m_TaaPass = new TAAPass();
            m_TaaDataCaches = new Dictionary<Camera, TAAData>();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType is not (CameraType.Game)) return;
            if (isFirstFrame)
            {
                isFirstFrame = false;
                return;
            }
            // Update TAA data
            Camera camera = renderingData.cameraData.camera;
            if (!m_TaaDataCaches.TryGetValue(camera, out var taaData))
            {
                taaData = new TAAData();
                m_TaaDataCaches.Add(camera, taaData);
            }
            UpdateTaaData(camera, taaData);

            // Camera setup pass
            m_CameraSetupPass.Setup(taaData);
            renderer.EnqueuePass(m_CameraSetupPass);

            // TAA pass
            m_TaaPass.Setup(taaData);
            renderer.EnqueuePass(m_TaaPass);
        }

        private void UpdateTaaData(Camera camera, TAAData taaData)
        {
            Vector2 jitter = TAAUtils.GetHaltonSequence9() * jitterIntensity;
            taaData.offset = new Vector2(jitter.x / camera.scaledPixelWidth, jitter.y / camera.scaledPixelHeight);
            taaData.projPreview = projPreview;
            taaData.viewPreview = viewPreview;
            taaData.projJitter = camera.orthographic
                ? TAAUtils.GetJitteredOrthographicProjectionMatrix(camera, jitter)
                : TAAUtils.GetJitteredPerspectiveProjectionMatrix(camera, jitter);
            taaData.blend = blend;
            taaData.quality = quality;

            projPreview = camera.projectionMatrix;
            viewPreview = camera.worldToCameraMatrix;
        }
    }
}