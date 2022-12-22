using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TAA
{
    public class TAACameraSetupPass : ScriptableRenderPass
    {
        private TAAData m_TaaData;

        internal TAACameraSetupPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        internal void Setup(TAAData taaData)
        {
            m_TaaData = taaData;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("TAA Camera Setup");
            using (new ProfilingScope(cmd, new ProfilingSampler("TAA Camera Setup")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                Camera camera = renderingData.cameraData.camera;
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, m_TaaData.projJitter);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}