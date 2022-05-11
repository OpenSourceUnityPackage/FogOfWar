using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FogOfWarPackage
{
    public class PostProcessFogOfWarFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class FeatureSettings
        {
            public bool IsEnabled = true;
            public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
            public Material material;
            public TerrainFogOfWar[] terrainFogOfWars;
        }

        public FeatureSettings settings = new FeatureSettings();

        PostProcessFogOfWar m_fogRenderPass;

        public override void Create()
        {
            if (Application.isPlaying)
            {
                m_fogRenderPass = new PostProcessFogOfWar(
                    "Fog of war pass",
                    settings.WhenToInsert,
                    settings.material,
                    settings.terrainFogOfWars
                );
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.IsEnabled || !Application.isPlaying)
                return;

            m_fogRenderPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_fogRenderPass);
        }
    }
}