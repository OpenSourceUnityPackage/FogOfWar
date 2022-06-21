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
            public RenderPassEvent WhenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
            [HideInInspector] public TerrainFogOfWar[] terrainFogOfWars;
        }

        public FeatureSettings settings = new FeatureSettings();

        private PostProcessFogOfWar m_fogRenderPass;

        public void Awake()
        {
            // default value
            if (settings.shader == null)
                settings.shader = Shader.Find("PostProcess/URPFogOfWar");
        }

        public override void Create()
        {
            if (Application.isPlaying)
            {
                m_fogRenderPass = new PostProcessFogOfWar(
                    "Fog of war pass",
                    settings.WhenToInsert,
                    new Material(settings.shader),
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