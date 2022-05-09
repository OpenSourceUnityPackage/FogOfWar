using FogOfWarPackage;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessFowOfWarFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MyFeatureSettings
    {
        public bool IsEnabled = true;
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
        public Material material;
    }

    public MyFeatureSettings settings = new MyFeatureSettings();

    PostProcessFowOfWar myRenderPass;


    public override void Create()
    {
        var fow = FindObjectsOfType<TerrainFogOfWar>();
        myRenderPass = new PostProcessFowOfWar(
            "Fog of war pass",
            settings.WhenToInsert,
            settings.material,
            fow[0]
        );
    }
  
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.IsEnabled)
            return;
    
        myRenderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(myRenderPass);
    }
}