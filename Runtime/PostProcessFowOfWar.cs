using FogOfWarPackage;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessFowOfWar : ScriptableRenderPass
{
  string profilerTag;

  Material materialToBlit;
  RenderTargetIdentifier source;
  RenderTargetHandle tempTexture;
  TerrainFogOfWar terrainFogOfWar;
  
  private static readonly int FogOfWarProp = Shader.PropertyToID("_FogOfWar");
  private static readonly int TerrainSizePosProp = Shader.PropertyToID("_TerrainSizePos");

  public PostProcessFowOfWar(string profilerTag,
    RenderPassEvent renderPassEvent, Material materialToBlit, TerrainFogOfWar terrainFogOfWar)
  {
    this.profilerTag = profilerTag;
    this.renderPassEvent = renderPassEvent;
    this.materialToBlit = materialToBlit;
    this.terrainFogOfWar = terrainFogOfWar;
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
    Terrain terrain = terrainFogOfWar.Terrain;

    if (terrain == null)
      return;
    TerrainData terrainData = terrain.terrainData;
    
    CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
    cmd.Clear();

    materialToBlit.SetTexture(FogOfWarProp, terrainFogOfWar.RenderTexture);
    materialToBlit.SetVector(TerrainSizePosProp, new Vector4(terrain.GetPosition().x, terrain.GetPosition().z, 1 / terrainData.size.x, 1 / terrainData.size.z));
    
    cmd.Blit(source, tempTexture.Identifier(), materialToBlit, 0);
    cmd.Blit(tempTexture.Identifier(), source);
    
    context.ExecuteCommandBuffer(cmd);

    cmd.Clear();
    CommandBufferPool.Release(cmd);
  }

  public override void FrameCleanup(CommandBuffer cmd)
  {
    cmd.ReleaseTemporaryRT(tempTexture.id);
  }
}