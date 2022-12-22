using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TAA
{
    internal static class ShaderKeywordStrings
    {
        internal static readonly string _HIGH_TAA = "_HIGH_TAA";
        internal static readonly string _MIDDLE_TAA = "_MIDDLE_TAA";
        internal static readonly string _LOW_TAA = "_LOW_TAA";
    }

    internal static class ShaderConstants
    {
        public static readonly int _Jitter_Blend = Shader.PropertyToID("_Jitter_Blend");
        public static readonly int _TAA_PreTexture = Shader.PropertyToID("_TAA_PreTexture");
        public static readonly int _Preview_VP = Shader.PropertyToID("_Preview_VP");
        public static readonly int _TAA_CurInvView = Shader.PropertyToID("_Inv_view_jittered");
        public static readonly int _TAA_CurInvProj = Shader.PropertyToID("_Inv_proj_jittered");
    }

    public class TAAPass : ScriptableRenderPass
    {
        const string k_TaaShader = "TAA";

        private TAAData m_TaaData;
        private Material m_Material;
        private Material material
        {
            get
            {
                if (m_Material == null)
                {
                    m_Material = new Material(Shader.Find(k_TaaShader));
                }
                return m_Material;
            }
        }

        private RenderTexture[] historyBuffer;
        private static int s_IndexWrite = 0;

        internal TAAPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        internal void Setup(TAAData taaData)
        {
            m_TaaData = taaData;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("TAA Pass");
            using (new ProfilingScope(cmd, new ProfilingSampler("TAA Pass")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var camera = renderingData.cameraData.camera;
                var colorTextureIdentifier = renderingData.cameraData.renderer.cameraColorTarget;
                var descriptor = new RenderTextureDescriptor(camera.scaledPixelWidth, camera.scaledPixelHeight, RenderTextureFormat.DefaultHDR, 16);
                TAAUtils.EnsureArray(ref historyBuffer, 2);
                TAAUtils.EnsureRenderTarget(ref historyBuffer[0], descriptor.width, descriptor.height, descriptor.colorFormat, FilterMode.Bilinear);
                TAAUtils.EnsureRenderTarget(ref historyBuffer[1], descriptor.width, descriptor.height, descriptor.colorFormat, FilterMode.Bilinear);

                int indexRead = s_IndexWrite;
                s_IndexWrite = ++s_IndexWrite % 2;

                Matrix4x4 inv_p_jittered = Matrix4x4.Inverse(m_TaaData.projJitter);
                Matrix4x4 inv_v_jittered = Matrix4x4.Inverse(camera.worldToCameraMatrix);
                Matrix4x4 preview_vp = m_TaaData.projPreview * m_TaaData.viewPreview;
                material.SetMatrix(ShaderConstants._TAA_CurInvView, inv_v_jittered);
                material.SetMatrix(ShaderConstants._TAA_CurInvProj, inv_p_jittered);
                material.SetMatrix(ShaderConstants._Preview_VP, preview_vp);
                material.SetVector(ShaderConstants._Jitter_Blend, new Vector4(m_TaaData.offset.x, m_TaaData.offset.y, m_TaaData.blend, 0.0f));
                material.SetTexture(ShaderConstants._TAA_PreTexture, historyBuffer[indexRead]);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings._HIGH_TAA, m_TaaData.quality == TAAFeature.TAAQuality.High);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings._MIDDLE_TAA, m_TaaData.quality == TAAFeature.TAAQuality.Medium);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings._LOW_TAA, m_TaaData.quality == TAAFeature.TAAQuality.Low);
                cmd.Blit(colorTextureIdentifier, historyBuffer[s_IndexWrite], material);
                cmd.Blit(historyBuffer[s_IndexWrite], colorTextureIdentifier);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}