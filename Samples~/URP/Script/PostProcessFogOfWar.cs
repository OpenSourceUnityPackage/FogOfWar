using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FogOfWarPackage
{
    public class PostProcessFogOfWar : ScriptableRenderPass
    {
        private readonly string profilerTag;

        private readonly Material materialFOW;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        private readonly TerrainFogOfWar[] terrainsFogOfWar;

        private static readonly int FogOfWarProp = Shader.PropertyToID("_FogOfWar");
        private static readonly int TerrainSizePosProp = Shader.PropertyToID("_TerrainSizePos");

        public PostProcessFogOfWar(string profilerTag,
            RenderPassEvent renderPassEvent, Material materialFOW, TerrainFogOfWar[] terrainsFogOfWar)
        {
            this.profilerTag = profilerTag;
            this.renderPassEvent = renderPassEvent;
            this.materialFOW = materialFOW;
            this.terrainsFogOfWar = terrainsFogOfWar;
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (terrainsFogOfWar == null || terrainsFogOfWar.Length == 0)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
            {
                // Do post process pass for each terrain (TODO: find a way to optimize)
                foreach (TerrainFogOfWar terrainFogOfWar in terrainsFogOfWar)
                {
                    Terrain terrain = terrainFogOfWar.Terrain;
                    TerrainData terrainData = terrain.terrainData;
                    
                    cmd.SetGlobalTexture(FogOfWarProp, terrainFogOfWar.RenderTexture);
                    cmd.SetGlobalVector(TerrainSizePosProp,
                        new Vector4(-terrain.GetPosition().x, -terrain.GetPosition().z, 1 / terrainData.size.x,
                            1 / terrainData.size.z));

                    cmd.Blit(source, tempTexture.Identifier(), materialFOW, 0);
                    cmd.Blit(tempTexture.Identifier(), source);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}