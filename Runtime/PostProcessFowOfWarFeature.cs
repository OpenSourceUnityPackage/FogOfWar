using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FogOfWarPackage
{
    public class PostProcessFowOfWarFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class MyFeatureSettings
        {
            public bool IsEnabled = true;
            public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
            public Material material;
        }

        public MyFeatureSettings settings = new MyFeatureSettings();

        PostProcessFowOfWar FOWRenderPass;


        public override void Create()
        {
            if (Application.isPlaying)
            {
                TerrainFogOfWar[] fow = FindObjectsOfType<TerrainFogOfWar>();
                FOWRenderPass = new PostProcessFowOfWar(
                    "Fog of war pass",
                    settings.WhenToInsert,
                    settings.material,
                    fow
                );
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.IsEnabled || !Application.isPlaying)
                return;

            FOWRenderPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(FOWRenderPass);
        }
    }
}